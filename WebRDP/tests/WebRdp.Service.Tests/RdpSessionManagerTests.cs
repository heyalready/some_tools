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
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig { Host = "127.0.0.2", Port = 3389 };

            // Act
            var session = await manager.ConnectAsync(config);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(RdpSessionStatus.Connecting, session.Status);
            Assert.NotEmpty(session.Id);
            Assert.Equal(8, session.Id.Length); // Session ID should be 8 characters
        }

        [Fact]
        public async Task ConnectAsync_WhenExistingConnectedSession_ReturnsExistingSession()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig { Host = "127.0.0.2", Port = 3389 };

            // Act
            var session1 = await manager.ConnectAsync(config);
            session1.Status = RdpSessionStatus.Connected;
            
            var session2 = await manager.ConnectAsync(config);

            // Assert
            Assert.Same(session1, session2);
        }

        [Fact]
        public async Task DisconnectAsync_CallsClientDisconnect()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            // Act
            var session = await manager.ConnectAsync(config);
            await manager.DisconnectAsync(session.Id);

            // Assert
            clientMock.Verify(c => c.DisconnectAsync(), Times.Once);
            clientMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task GetStatusAsync_WithValidSessionId_ReturnsCorrectStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            // Act
            var session = await manager.ConnectAsync(config);
            var status = await manager.GetStatusAsync(session.Id);

            // Assert
            Assert.Equal(session.Status, status);
        }

        [Fact]
        public async Task GetStatusAsync_WithInvalidSessionId_ReturnsDisconnected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);

            // Act
            var status = await manager.GetStatusAsync("invalid-session-id");

            // Assert
            Assert.Equal(RdpSessionStatus.Disconnected, status);
        }

        [Fact]
        public async Task GetExistingSession_WhenNoConnectedSession_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);

            // Act
            var existing = manager.GetExistingSession();

            // Assert
            Assert.Null(existing);
        }

        [Fact]
        public async Task ConnectAsync_IsThreadSafe()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            // Act
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => Task.Run(() => manager.ConnectAsync(config)))
                .ToArray();
            
            var sessions = await Task.WhenAll(tasks);

            // Assert - All sessions should be the same instance due to thread safety
            Assert.NotNull(sessions);
            Assert.All(sessions, s => Assert.NotNull(s));
            Assert.All(sessions, s => Assert.Same(sessions[0], s));
        }

        #region Additional Test Cases for Missing Scenarios

        [Fact]
        public async Task DisconnectAsync_WithInvalidSessionId_DoesNothing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);

            // Act
            await manager.DisconnectAsync("non-existent-session");

            // Assert
            clientMock.Verify(c => c.DisconnectAsync(), Times.Never);
            clientMock.Verify(c => c.Dispose(), Times.Never);
        }

        [Fact]
        public async Task ConnectAsync_WhenSessionInConnectingState_CreatesNewSession()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig { Host = "127.0.0.2", Port = 3389 };

            // Act - First connection (status will be Connecting)
            var session1 = await manager.ConnectAsync(config);
            // Don't change status, keep it as Connecting
            
            // Second connection while first is still connecting
            var session2 = await manager.ConnectAsync(config);

            // Assert - Should return the same session even if still connecting
            Assert.Same(session1, session2);
        }

        [Fact]
        public async Task ConnectAsync_WhenMaxSessionsReached_ReplacesOldSession()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings { MaxSessionCount = 1 });
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config1 = new RdpConnectionConfig { Host = "127.0.0.1", Port = 3389 };
            var config2 = new RdpConnectionConfig { Host = "127.0.0.2", Port = 3389 };

            // Act
            var session1 = await manager.ConnectAsync(config1);
            session1.Status = RdpSessionStatus.Error; // Simulate error state
            
            var session2 = await manager.ConnectAsync(config2);

            // Assert
            Assert.NotNull(session2);
            Assert.NotEqual(session1.Id, session2.Id); // New session should have different ID
        }

        [Fact]
        public async Task GetExistingSession_WhenSessionIsConnecting_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            // Act
            var session = await manager.ConnectAsync(config);
            // Session status is Connecting at this point
            var existing = manager.GetExistingSession();

            // Assert
            Assert.Null(existing); // Only Connected sessions should be returned
        }

        [Fact]
        public async Task GetExistingSession_WhenSessionIsConnected_ReturnsSession()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            // Act
            var session = await manager.ConnectAsync(config);
            session.Status = RdpSessionStatus.Connected;
            var existing = manager.GetExistingSession();

            // Assert
            Assert.NotNull(existing);
            Assert.Same(session, existing);
        }

        [Fact]
        public async Task ConnectAsync_SetsCorrectSessionProperties()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig 
            { 
                Host = "192.168.1.100", 
                Port = 3390,
                Username = "testuser",
                Width = 1920,
                Height = 1080
            };

            // Act
            var session = await manager.ConnectAsync(config);

            // Assert
            Assert.Equal(RdpSessionStatus.Connecting, session.Status);
            Assert.Equal(config.Host, session.Config.Host);
            Assert.Equal(config.Port, session.Config.Port);
            Assert.InRange(session.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            Assert.InRange(session.LastActiveAt!.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public async Task DisconnectAsync_AfterDisconnect_SessionIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RdpSessionManager>>();
            var clientMock = new Mock<IFreeRdpClient>();
            var clientFactoryMock = new Mock<IFreeRdpClientFactory>();
            clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);
            var settings = Options.Create(new RdpSettings());
            
            var manager = new RdpSessionManager(loggerMock.Object, clientFactoryMock.Object, settings);
            var config = new RdpConnectionConfig();

            // Act
            var session = await manager.ConnectAsync(config);
            await manager.DisconnectAsync(session.Id);
            var existingAfterDisconnect = manager.GetExistingSession();

            // Assert
            Assert.Null(existingAfterDisconnect);
        }

        #endregion
    }
}
