using Microsoft.Extensions.Logging;

namespace WebRdp.Client
{
    /// <summary>
    /// FreeRDP 客户端工厂实现
    /// </summary>
    public class FreeRdpClientFactory : IFreeRdpClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public FreeRdpClientFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IFreeRdpClient CreateClient()
        {
            var logger = _loggerFactory.CreateLogger<FreeRdpClient>();
            return new FreeRdpClient(logger);
        }
    }
}
