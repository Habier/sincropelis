using FluentAssertions;

namespace SincroPelis.Tests.Unit;

public class KeyControllerTests
{
    [Fact]
    public void ShortcutEnum_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues(typeof(KeyController.Shortcut));
        
        values.Length.Should().Be(6);
    }

    [Fact]
    public void ShortcutEnum_ShouldContainPlayPause()
    {
        var hasPlayPause = Enum.IsDefined(typeof(KeyController.Shortcut), KeyController.Shortcut.PlayPause);
        
        hasPlayPause.Should().BeTrue();
    }

    [Fact]
    public void ShortcutEnum_ShouldContainSeekBack()
    {
        var hasSeekBack = Enum.IsDefined(typeof(KeyController.Shortcut), KeyController.Shortcut.SeekBack);
        
        hasSeekBack.Should().BeTrue();
    }

    [Fact]
    public void ShortcutEnum_ShouldContainSeekForward()
    {
        var hasSeekForward = Enum.IsDefined(typeof(KeyController.Shortcut), KeyController.Shortcut.SeekForward);
        
        hasSeekForward.Should().BeTrue();
    }

    [Fact]
    public void ShortcutEnum_ShouldContainEscape()
    {
        var hasEscape = Enum.IsDefined(typeof(KeyController.Shortcut), KeyController.Shortcut.Escape);
        
        hasEscape.Should().BeTrue();
    }

    [Fact]
    public void ShortcutEnum_ShouldContainVolumeUp()
    {
        var hasVolumeUp = Enum.IsDefined(typeof(KeyController.Shortcut), KeyController.Shortcut.VolumeUp);
        
        hasVolumeUp.Should().BeTrue();
    }

    [Fact]
    public void ShortcutEnum_ShouldContainVolumeDown()
    {
        var hasVolumeDown = Enum.IsDefined(typeof(KeyController.Shortcut), KeyController.Shortcut.VolumeDown);
        
        hasVolumeDown.Should().BeTrue();
    }

    [Fact]
    public void Start_ShouldNotThrow()
    {
        var action = () => KeyController.Start();
        
        action.Should().NotThrow();
        
        KeyController.Stop();
    }

    [Fact]
    public void Log_ShouldNotThrow()
    {
        Logger.Initialize();
        
        var action = () => KeyController.Log("Test message");
        
        action.Should().NotThrow();
        
        Logger.Shutdown();
    }
}
