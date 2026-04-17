using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebRdp.Client;
using WebRdp.Service.Models;

namespace WebRdp.Service.Services
{
    public interface IRdpSessionManager
    {
        Task<RdpSession> ConnectAsync(RdpConnectionConfig config);
        Task DisconnectAsync(string sessionId);
        Task<RdpSessionStatus> GetStatusAsync(string sessionId);
        RdpSession? GetExistingSession();
    }

    public class RdpSessionManager : IRdpSessionManager
    {
        private readonly ILogger<RdpSessionManager> _logger;
        private readonly IFreeRdpClientFactory _clientFactory;
        private readonly RdpSettings _settings;
        private readonly SemaphoreSlim _sessionLock;
        private RdpSession? _currentSession;
        private IFreeRdpClient? _currentClient;

        public RdpSessionManager(
            ILogger<RdpSessionManager> logger,
            IFreeRdpClientFactory clientFactory,
            IOptions<RdpSettings> settings)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _settings = settings.Value;
            _sessionLock = new SemaphoreSlim(1, 1);
        }

        public async Task<RdpSession> ConnectAsync(RdpConnectionConfig config)
        {
            await _sessionLock.WaitAsync();
            try
            {
                _logger.LogInformation("Connect request received");

                if (_currentSession != null && _currentSession.Status == RdpSessionStatus.Connected)
                {
                    _logger.LogInformation("Returning existing session");
                    _currentSession.LastActiveAt = DateTime.UtcNow;
                    return _currentSession;
                }
                
                if (_currentSession != null)
                {
                    _logger.LogInformation("Cleaning up old session");
                    await CleanupSessionAsync();
                }

                var sessionId = Guid.NewGuid().ToString("N")[..8];
                _currentSession = new RdpSession
                {
                    Id = sessionId,
                    Status = RdpSessionStatus.Connecting,
                    CreatedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,
                    Config = config
                };

                _logger.LogInformation($"Creating new session {sessionId}");
                _ = ConnectInBackgroundAsync(config);

                return _currentSession;
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        private async Task ConnectInBackgroundAsync(RdpConnectionConfig config)
        {
            try
            {
                _currentClient = _clientFactory.CreateClient();
                
                _currentClient.ConnectionStateChanged += (s, e) =>
                {
                    if (_currentSession != null)
                    {
                        _currentSession.Status = e.NewState switch
                        {
                            Native.ConnectionState.Connected => RdpSessionStatus.Connected,
                            Native.ConnectionState.Disconnected => RdpSessionStatus.Disconnected,
                            Native.ConnectionState.Error => RdpSessionStatus.Error,
                            _ => _currentSession.Status
                        };
                    }
                };

                await _currentClient.ConnectAsync(config);
                _logger.LogInformation("Background connection completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background connection failed");
                if (_currentSession != null)
                {
                    _currentSession.Status = RdpSessionStatus.Error;
                }
            }
        }

        public async Task DisconnectAsync(string sessionId)
        {
            await _sessionLock.WaitAsync();
            try
            {
                if (_currentSession?.Id != sessionId)
                {
                    _logger.LogWarning($"Session {sessionId} not found");
                    return;
                }

                _logger.LogInformation($"Disconnecting session {sessionId}");
                await CleanupSessionAsync();
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        public Task<RdpSessionStatus> GetStatusAsync(string sessionId)
        {
            if (_currentSession?.Id == sessionId)
            {
                return Task.FromResult(_currentSession.Status);
            }
            return Task.FromResult(RdpSessionStatus.Disconnected);
        }

        public RdpSession? GetExistingSession()
        {
            return _currentSession?.Status == RdpSessionStatus.Connected ? _currentSession : null;
        }

        private async Task CleanupSessionAsync()
        {
            if (_currentClient != null)
            {
                await _currentClient.DisconnectAsync();
                _currentClient.Dispose();
                _currentClient = null;
            }
            _currentSession = null;
            _logger.LogInformation("Session cleaned up");
        }
    }
}
