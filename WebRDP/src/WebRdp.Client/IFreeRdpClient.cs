using System;
using System.Threading.Tasks;

namespace WebRdp.Client
{
    /// <summary>
    /// FreeRDP 客户端接口
    /// </summary>
    public interface IFreeRdpClient : IDisposable
    {
        /// <summary>
        /// 帧数据就绪事件
        /// </summary>
        event EventHandler<FrameEventArgs>? FrameReady;

        /// <summary>
        /// 连接状态变更事件
        /// </summary>
        event EventHandler<ConnectionEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// 连接到 RDP 服务器
        /// </summary>
        Task ConnectAsync(RdpConnectionConfig config);

        /// <summary>
        /// 发送输入事件
        /// </summary>
        Task SendInputAsync(InputEvent input);

        /// <summary>
        /// 断开连接
        /// </summary>
        Task DisconnectAsync();
    }

    /// <summary>
    /// FreeRDP 客户端工厂接口
    /// </summary>
    public interface IFreeRdpClientFactory
    {
        IFreeRdpClient CreateClient();
    }

    /// <summary>
    /// RDP 连接配置
    /// </summary>
    public class RdpConnectionConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3389;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int ColorDepth { get; set; } = 32;
        public string LocalSessionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 帧数据事件参数
    /// </summary>
    public class FrameEventArgs : EventArgs
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 连接状态事件参数
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        public Native.ConnectionState OldState { get; set; }
        public Native.ConnectionState NewState { get; set; }
    }

    /// <summary>
    /// 输入事件结构（用于 SendInputAsync）
    /// </summary>
    public class InputEvent
    {
        public Native.InputEventType EventType { get; set; }
        public int KeyCode { get; set; }
        public int MouseX { get; set; }
        public int MouseY { get; set; }
        public int Flags { get; set; }
    }

    /// <summary>
    /// RDP 连接异常
    /// </summary>
    public class RdpConnectionException : Exception
    {
        public RdpConnectionException(string message) : base(message) { }
        public RdpConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
