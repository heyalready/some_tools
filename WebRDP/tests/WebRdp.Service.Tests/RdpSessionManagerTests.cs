using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebRdp.Service.Services;
using WebRdp.Client;
using WebRdp.Service.Models;

namespace WebRdp.Service.Tests
{
    public class RdpSessionManagerTests
    {
        [Fact]
        public async Task ConnectAsync_WhenNoExistingSession_CreatesNewSession()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig { Host = "127.0.0.2", Port = 3389 };

            var session = await manager.ConnectAsync(config);

            Assert.NotNull(session);
            Assert.Equal(RdpSessionStatus.Connecting, session.Status);
            Assert.NotEmpty(session.Id);
        }

        [Fact]
        public async Task ConnectAsync_WhenExistingConnectedSession_ReturnsExistingSession()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig { Host = "127.0.0.2", Port = 3389 };

            var session1 = await manager.ConnectAsync(config);
            session1.Status = RdpSessionStatus.Connected;
            
            var session2 = await manager.ConnectAsync(config);

            Assert.Same(session1, session2);
        }

        [Fact]
        public async Task DisconnectAsync_CallsClientDisconnect()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            var session = await manager.ConnectAsync(config);
            await manager.DisconnectAsync(session.Id);

            clientMock.Verify(c => c.DisconnectAsync(), Times.Once);
            clientMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task GetStatusAsync_WithValidSessionId_ReturnsCorrectStatus()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            var session = await manager.ConnectAsync(config);
            var status = await manager.GetStatusAsync(session.Id);

            Assert.Equal(session.Status, status);
        }

        [Fact]
        public async Task GetStatusAsync_WithInvalidSessionId_ReturnsDisconnected()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);

            var status = await manager.GetStatusAsync("invalid-session-id");

            Assert.Equal(RdpSessionStatus.Disconnected, status);
        }

        [Fact]
        public async Task GetExistingSession_WhenNoConnectedSession_ReturnsNull()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);

            var existing = manager.GetExistingSession();

            Assert.Null(existing);
        }

        [Fact]
        public async Task ConnectAsync_IsThreadSafe()
        {
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            var tasks = Enumerable.Range(0, 5)
                .Select(_ => Task.Run(() => manager.ConnectAsync(config)))
                .ToArray();
            
            var sessions = await Task.WhenAll(tasks);

            Assert.NotNull(sessions);
            Assert.All(sessions, s => Assert.NotNull(s));
        }
    }
}
