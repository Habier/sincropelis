using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Sockets;

namespace SincroPelis.Tests.Unit;

public class ServerTests : IDisposable
{
    private int _originalPort;

    public ServerTests()
    {
        Logger.Initialize();
        _originalPort = Server.PORT;
    }

    [Fact]
    public void Server_Constructor_ShouldNotThrow()
    {
        var action = () => new Server();
        
        action.Should().NotThrow();
    }

    [Fact]
    public void Server_ShouldBeAccessible()
    {
        var serverType = typeof(Server);
        serverType.Should().NotBeNull();
    }

    [Fact]
    public void DefaultPort_ShouldBeDefined()
    {
        Server.PORT.Should().BeGreaterThan(0);
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        Server.PORT = _originalPort;
        Logger.Shutdown();
    }
}
