using FluentAssertions;
using System.Net;
using System.Net.Sockets;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SincroPelis.Tests.Unit;

[Collection("Sequential")]
public class ClientTests : IDisposable
{
    private int _originalPort;

    public ClientTests()
    {
        Logger.Initialize();
        _originalPort = Client.PORT;
    }

    [Fact]
    public void Client_ShouldBeAccessible()
    {
        var clientType = typeof(Client);
        clientType.Should().NotBeNull();
    }

    [Fact]
    public void DefaultPort_ShouldBe9000()
    {
        Client.PORT.Should().Be(9000);
    }

    [Fact]
    public void TryConnect_ShouldNotThrow_WhenInvalidIp()
    {
        var client = new Client();
        
        var action = () => client.TryConnect("256.256.256.256");
        
        action.Should().NotThrow();
    }

    [Fact]
    public void TrySend_ShouldNotThrow_WhenNotConnected()
    {
        var client = new Client();
        
        var action = () => client.TrySend("test");
        
        action.Should().NotThrow();
    }

    [Fact]
    public void TryConnect_ShouldNotThrow_WhenValidIpButNoServer()
    {
        var client = new Client();
        Client.PORT = GetAvailablePort();
        
        var action = () => client.TryConnect("127.0.0.1");
        
        action.Should().NotThrow();
    }

    [Fact]
    public void TrySend_ShouldWorkWithEmptyMessage()
    {
        var client = new Client();
        
        var action = () => client.TrySend();
        
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
        Client.PORT = _originalPort;
        Logger.Shutdown();
    }
}
