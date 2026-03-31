using FluentAssertions;
using System.IO;

namespace SincroPelis.Tests.Unit;

public class LoggerTests : IDisposable
{
    private readonly string _testLogDir;

    public LoggerTests()
    {
        _testLogDir = Path.Combine(Path.GetTempPath(), "SincroPelis_Tests", Guid.NewGuid().ToString());
    }

    [Fact]
    public void Initialize_ShouldNotThrow()
    {
        var action = () => Logger.Initialize();
        
        action.Should().NotThrow();
    }

    [Fact]
    public void Info_ShouldNotThrow()
    {
        Logger.Initialize();
        
        var action = () => Logger.Info("Test info message");
        
        action.Should().NotThrow();
        
        Logger.Shutdown();
    }

    [Fact]
    public void Debug_ShouldNotThrow()
    {
        Logger.Initialize();
        
        var action = () => Logger.Debug("Test debug message");
        
        action.Should().NotThrow();
        
        Logger.Shutdown();
    }

    [Fact]
    public void Warning_ShouldNotThrow()
    {
        Logger.Initialize();
        
        var action = () => Logger.Warning("Test warning message");
        
        action.Should().NotThrow();
        
        Logger.Shutdown();
    }

    [Fact]
    public void Error_ShouldNotThrow_WhenMessageOnly()
    {
        Logger.Initialize();
        
        var action = () => Logger.Error("Test error message");
        
        action.Should().NotThrow();
        
        Logger.Shutdown();
    }

    [Fact]
    public void Error_ShouldNotThrow_WhenMessageAndException()
    {
        Logger.Initialize();
        
        var exception = new InvalidOperationException("Test exception");
        var action = () => Logger.Error("Test error with exception", exception);
        
        action.Should().NotThrow();
        
        Logger.Shutdown();
    }

    [Fact]
    public void Shutdown_ShouldNotThrow()
    {
        Logger.Initialize();
        
        var action = () => Logger.Shutdown();
        
        action.Should().NotThrow();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testLogDir))
            {
                Directory.Delete(_testLogDir, true);
            }
        }
        catch { }
    }
}
