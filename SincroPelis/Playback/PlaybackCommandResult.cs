namespace SincroPelis.Playback
{
    public sealed class PlaybackCommandResult
    {
        public static PlaybackCommandResult None { get; } = new PlaybackCommandResult(false, null, false);

        public PlaybackCommandResult(bool handled, string? playPauseText, bool resetPosition)
        {
            Handled = handled;
            PlayPauseText = playPauseText;
            ResetPosition = resetPosition;
        }

        public bool Handled { get; }
        public string? PlayPauseText { get; }
        public bool ResetPosition { get; }
    }
}
