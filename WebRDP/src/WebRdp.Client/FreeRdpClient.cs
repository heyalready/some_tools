using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebRdp.Client.Native;

namespace WebRdp.Client
{
    /// <summary>
    /// FreeRDP 客户端封装
    /// 提供 RDP 协议的.NET 接口
    /// </summary>
    public class FreeRdpClient : IFreeRdpClient
    {
        private readonly ILogger<FreeRdpClient> _logger;
        private IntPtr _context;
        private bool _disposed;
        private bool _isConnected;

        /// <summary>
        /// 帧数据就绪事件
        /// </summary>
        public event EventHandler<FrameEventArgs>? FrameReady;

        /// <summary>
        /// 连接状态变更事件
        /// </summary>
        public event EventHandler<ConnectionEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FreeRdpClient(ILogger<FreeRdpClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = IntPtr.Zero;
        }

        /// <summary>
        /// 连接到 RDP 服务器
        /// </summary>
        public async Task ConnectAsync(RdpConnectionConfig config)
        {
            if (_isConnected)
            {
                _logger.LogWarning("Already connected, disconnecting first...");
                await DisconnectAsync();
            }

            try
            {
                _logger.LogInformation($"Connecting to {config.Host}:{config.Port} as {config.Username}");
                
                // 初始化 FreeRDP 上下文
                _context = FreerdpInterop.freerdp_context_new();
                if (_context == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to create FreeRDP context");
                }

                // 配置连接参数
                var settings = new FREERDP_SETTINGS
                {
                    ServerHostname = config.Host,
                    ServerPort = config.Port,
                    Username = config.Username,
                    Password = config.Password,
                    Width = config.Width,
                    Height = config.Height,
                    ColorDepth = config.ColorDepth,
                    LocalSessionId = config.LocalSessionId
                };

                FreerdpInterop.freerdp_context_set_settings(_context, settings);

                // 注册事件回调
                FreerdpInterop.freerdp_context_set_frame_callback(_context, OnFrameReceived);
                FreerdpInterop.freerdp_context_set_state_callback(_context, OnStateChanged);

                // 异步连接
                await Task.Run(() =>
                {
                    var result = FreerdpInterop.freerdp_context_connect(_context);
                    if (result != 0)
                    {
                        throw new RdpConnectionException($"Connection failed with error code {result}");
                    }
                });

                _isConnected = true;
                _logger.LogInformation("Connected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection failed");
                await DisconnectAsync();
                throw;
            }
        }

        /// <summary>
        /// 发送输入事件
        /// </summary>
        public async Task SendInputAsync(InputEvent input)
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("Not connected");
            }

            await Task.Run(() =>
            {
                var nativeInput = new FREERDP_INPUT_EVENT
                {
                    EventType = (int)input.EventType,
                    KeyCode = input.KeyCode,
                    MouseX = input.MouseX,
                    MouseY = input.MouseY,
                    Flags = input.Flags
                };

                FreerdpInterop.freerdp_context_send_input(_context, nativeInput);
            });
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_context != IntPtr.Zero && !_disposed)
            {
                await Task.Run(() =>
                {
                    FreerdpInterop.freerdp_context_disconnect(_context);
                    FreerdpInterop.freerdp_context_free(_context);
                });
                
                _context = IntPtr.Zero;
                _isConnected = false;
                _logger.LogInformation("Disconnected");
            }
        }

        /// <summary>
        /// 帧数据回调
        /// </summary>
        private void OnFrameReceived(IntPtr frameData, int width, int height, int stride)
        {
            try
            {
                var frameDataArray = new byte[width * height * 4];
                Marshal.Copy(frameData, frameDataArray, 0, frameDataArray.Length);
                
                FrameReady?.Invoke(this, new FrameEventArgs
                {
                    Data = frameDataArray,
                    Width = width,
                    Height = height,
                    Stride = stride,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing frame");
            }
        }

        /// <summary>
        /// 连接状态变更回调
        /// </summary>
        private void OnStateChanged(int oldState, int newState)
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionEventArgs
            {
                OldState = (ConnectionState)oldState,
                NewState = (ConnectionState)newState
            });
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    DisconnectAsync().Wait();
                }
                catch
                {
                    // Ignore dispose errors
                }
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
