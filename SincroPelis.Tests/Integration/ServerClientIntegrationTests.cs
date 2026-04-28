using FluentAssertions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SincroPelis.Tests.Integration;

public class ServerClientIntegrationTests : IDisposable
{
    private Server? _server;
    private Client? _client;
    private int _testPort;

    public ServerClientIntegrationTests()
    {
        Logger.Initialize();
        _testPort = GetAvailablePort();
    }

    [Fact]
    public void Server_ShouldExist()
    {
        _server = new Server();
        
        _server.Should().NotBeNull();
    }

    [Fact]
    public void StartAndConnect_ShouldEstablishConnection()
    {
        _server = new Server(_testPort);
        _client = new Client(_testPort);
        
        var action = () =>
        {
            _server.start();
            _client.TryConnect("127.0.0.1");
        };
        
        action.Should().NotThrow();
    }

    [Fact]
    public void MultipleClients_CanConnect()
    {
        _server = new Server(_testPort);
        _server.start();
        
        var client1 = new Client(_testPort);
        var client2 = new Client(_testPort);
        
        var action1 = () => client1.TryConnect("127.0.0.1");
        var action2 = () => client2.TryConnect("127.0.0.1");
        
        action1.Should().NotThrow();
        action2.Should().NotThrow();
    }

    [Fact]
    public void Client_TrySend_ShouldNotThrow_WhenDisconnected()
    {
        var client = new Client();
        
        var action = () => client.TrySend("pause");
        
        action.Should().NotThrow();
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
        try
        {
            _client?.TrySend("stop");
        }
        catch { }

        Logger.Shutdown();
    }
}
