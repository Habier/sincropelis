using FluentAssertions;
using SincroPelis.Playback;

namespace SincroPelis.Tests.Unit;

public class PlaybackServiceTests
{
    [Fact]
    public void TryGetActiveMediaPlayer_ShouldReturnFalse_WhenNotInitialized()
    {
        var service = new PlaybackService();

        var result = service.TryGetActiveMediaPlayer(out var mediaPlayer);

        result.Should().BeFalse();
        mediaPlayer.Should().BeNull();
    }

    [Fact]
    public void TogglePlayPause_ShouldReturnFalse_WhenNotInitialized()
    {
        var service = new PlaybackService();

        var result = service.TogglePlayPause();

        result.Should().BeFalse();
    }

    [Fact]
    public void GetVolume_ShouldReturnZero_WhenNotInitialized()
    {
        var service = new PlaybackService();

        service.GetVolume().Should().Be(0);
    }

    [Fact]
    public void GetAudioTracks_ShouldBeEmpty_WhenNotInitialized()
    {
        var service = new PlaybackService();

        service.GetAudioTracks().Should().BeEmpty();
    }

    [Fact]
    public void GetSubtitleTracks_ShouldContainOnlyNone_WhenNotInitialized()
    {
        var service = new PlaybackService();

        var tracks = service.GetSubtitleTracks();

        tracks.Should().ContainSingle();
        tracks[0].Name.Should().Be("None");
        tracks[0].Id.Should().Be(-1);
    }

    [Fact]
    public void HandleIncomingCommand_ShouldReturnPlayResult_AndEnableSuppression_ForPlay()
    {
        var service = new PlaybackService();

        var result = service.HandleIncomingCommand("play");

        result.Handled.Should().BeTrue();
        result.PlayPauseText.Should().Be("Play");
        result.ResetPosition.Should().BeFalse();
        service.ConsumeSuppressedSyncEvent().Should().BeTrue();
        service.ConsumeSuppressedSyncEvent().Should().BeFalse();
    }

    [Fact]
    public void HandleIncomingCommand_ShouldReturnPauseResult_AndEnableSuppression_ForPause()
    {
        var service = new PlaybackService();

        var result = service.HandleIncomingCommand("pause");

        result.Handled.Should().BeTrue();
        result.PlayPauseText.Should().Be("Pause");
        result.ResetPosition.Should().BeFalse();
        service.ConsumeSuppressedSyncEvent().Should().BeTrue();
        service.ConsumeSuppressedSyncEvent().Should().BeFalse();
    }

    [Fact]
    public void HandleIncomingCommand_ShouldResetPosition_AndEnableSuppression_ForStop()
    {
        var service = new PlaybackService();

        var result = service.HandleIncomingCommand("stop");

        result.Handled.Should().BeTrue();
        result.PlayPauseText.Should().Be("Play");
        result.ResetPosition.Should().BeTrue();
        service.ConsumeSuppressedSyncEvent().Should().BeTrue();
        service.ConsumeSuppressedSyncEvent().Should().BeFalse();
    }

    [Fact]
    public void HandleIncomingCommand_ShouldResetPosition_WithoutSuppression_ForEnded()
    {
        var service = new PlaybackService();

        var result = service.HandleIncomingCommand("ended");

        result.Handled.Should().BeTrue();
        result.PlayPauseText.Should().Be("Play");
        result.ResetPosition.Should().BeTrue();
        service.ConsumeSuppressedSyncEvent().Should().BeFalse();
    }

    [Theory]
    [InlineData("seek:abc")]
    [InlineData("seekby:nope")]
    [InlineData("audio:x")]
    [InlineData("sub:x")]
    [InlineData("unknown")]
    public void HandleIncomingCommand_ShouldReturnNone_ForInvalidCommands(string command)
    {
        var service = new PlaybackService();

        var result = service.HandleIncomingCommand(command);

        result.Handled.Should().BeFalse();
        result.PlayPauseText.Should().BeNull();
        result.ResetPosition.Should().BeFalse();
        service.ConsumeSuppressedSyncEvent().Should().BeFalse();
    }
}
