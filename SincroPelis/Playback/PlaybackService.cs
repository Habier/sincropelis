using LibVLCSharp.Shared;

namespace SincroPelis.Playback
{
    public sealed class PlaybackService : IDisposable
    {
        private LibVLC? _libVlc;
        private MediaPlayer? _mediaPlayer;
        private volatile bool _suppressSyncEvent;

        public MediaPlayer? MediaPlayer => _mediaPlayer;

        public bool Initialize(string? systemVlcLibPath, string? downloadedVlcLibPath)
        {
            if (!string.IsNullOrEmpty(systemVlcLibPath))
            {
                Core.Initialize(systemVlcLibPath);
            }
            else if (!string.IsNullOrEmpty(downloadedVlcLibPath))
            {
                Core.Initialize(downloadedVlcLibPath);
            }
            else
            {
                Core.Initialize();
            }

            try
            {
                _libVlc = new LibVLC(new[] { "--vout=direct3d11", "--no-video-title-show" });
            }
            catch
            {
                _libVlc = new LibVLC();
            }

            _mediaPlayer = new MediaPlayer(_libVlc);
            return true;
        }

        public bool TryGetActiveMediaPlayer(out MediaPlayer mediaPlayer)
        {
            mediaPlayer = null!;

            if (_mediaPlayer == null)
            {
                return false;
            }

            mediaPlayer = _mediaPlayer;
            return true;
        }

        public bool TogglePlayPause()
        {
            if (_mediaPlayer == null)
            {
                return false;
            }

            _suppressSyncEvent = true;

            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                return false;
            }

            _mediaPlayer.Play();
            return true;
        }

        public void Stop()
        {
            _mediaPlayer?.Stop();
        }

        public void SeekBySeconds(int seconds)
        {
            if (!TryGetActiveMediaPlayer(out var mediaPlayer))
            {
                return;
            }

            var newTime = Math.Max(0, Math.Min(mediaPlayer.Length, mediaPlayer.Time + (seconds * 1000L)));
            mediaPlayer.Time = newTime;
        }

        public void SeekToPosition(double position)
        {
            if (!TryGetActiveMediaPlayer(out var mediaPlayer) || mediaPlayer.Length <= 0)
            {
                return;
            }

            mediaPlayer.Position = (float)position;
        }

        public void SetVolume(int volume)
        {
            if (_mediaPlayer == null)
            {
                return;
            }

            _mediaPlayer.Volume = volume;
        }

        public int AdjustVolume(int delta)
        {
            if (_mediaPlayer == null)
            {
                return 0;
            }

            _mediaPlayer.Volume = Math.Max(0, Math.Min(100, _mediaPlayer.Volume + delta));
            return _mediaPlayer.Volume;
        }

        public int GetVolume() => _mediaPlayer?.Volume ?? 0;

        public bool IsPlaying() => _mediaPlayer?.IsPlaying ?? false;

        public bool ConsumeSuppressedSyncEvent()
        {
            if (!_suppressSyncEvent)
            {
                return false;
            }

            _suppressSyncEvent = false;
            return true;
        }

        public PlaybackCommandResult HandleIncomingCommand(string message)
        {
            if (message == "pause")
            {
                _suppressSyncEvent = true;
                _mediaPlayer?.Pause();
                return new PlaybackCommandResult(true, "Pause", false);
            }

            if (message == "play")
            {
                _suppressSyncEvent = true;
                _mediaPlayer?.Play();
                return new PlaybackCommandResult(true, "Play", false);
            }

            if (message == "stop")
            {
                _suppressSyncEvent = true;
                _mediaPlayer?.Stop();
                return new PlaybackCommandResult(true, "Play", true);
            }

            if (message == "ended")
            {
                return new PlaybackCommandResult(true, "Play", true);
            }

            if (message.StartsWith("seek:"))
            {
                var posStr = message.Substring(5);
                if (double.TryParse(posStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double pos))
                {
                    SeekToPosition(pos);
                    return new PlaybackCommandResult(true, null, false);
                }
            }

            if (message.StartsWith("seekby:"))
            {
                var secStr = message.Substring(7);
                if (int.TryParse(secStr, out int sec))
                {
                    SeekBySeconds(sec);
                    return new PlaybackCommandResult(true, null, false);
                }
            }

            if (message.StartsWith("audio:"))
            {
                var idStr = message.Substring(6);
                if (int.TryParse(idStr, out int id))
                {
                    SetAudioTrack(id);
                    return new PlaybackCommandResult(true, null, false);
                }
            }

            if (message.StartsWith("sub:"))
            {
                var idStr = message.Substring(4);
                if (int.TryParse(idStr, out int id))
                {
                    SetSubtitleTrack(id);
                    return new PlaybackCommandResult(true, null, false);
                }
            }

            return PlaybackCommandResult.None;
        }

        public bool LoadFile(string path, bool startPaused, Action<string> sendDebug)
        {
            if (_mediaPlayer == null || _libVlc == null)
            {
                sendDebug("No hay reproductor disponible: ");
                return false;
            }

            try
            {
                using var media = new Media(_libVlc, path, FromType.FromPath);
                if (startPaused)
                {
                    media.AddOption(":start-paused");
                }

                _mediaPlayer.Play(media);
                sendDebug(startPaused ? "Fichero cargado en LibVLC (pausado)." : "Reproduciendo en reproductor integrado (LibVLC).");
                return true;
            }
            catch (Exception ex)
            {
                sendDebug("Error cargando en LibVLC: " + ex.Message);
                return false;
            }
        }

        public IReadOnlyList<TrackOption> GetAudioTracks()
        {
            if (_mediaPlayer?.AudioTrackDescription == null)
            {
                return Array.Empty<TrackOption>();
            }

            return _mediaPlayer.AudioTrackDescription
                .Select(d => new TrackOption(string.IsNullOrEmpty(d.Name) ? $"Track {d.Id}" : d.Name, d.Id))
                .ToList();
        }

        public IReadOnlyList<TrackOption> GetSubtitleTracks()
        {
            var items = new List<TrackOption> { new TrackOption("None", -1) };

            if (_mediaPlayer?.SpuDescription == null)
            {
                return items;
            }

            items.AddRange(_mediaPlayer.SpuDescription
                .Select(d => new TrackOption(string.IsNullOrEmpty(d.Name) ? $"Sub {d.Id}" : d.Name, d.Id)));

            return items;
        }

        public int GetCurrentAudioTrack() => _mediaPlayer?.AudioTrack ?? -1;

        public int GetCurrentSubtitleTrack() => _mediaPlayer?.Spu ?? -1;

        public void SetAudioTrack(int id)
        {
            _mediaPlayer?.SetAudioTrack(id);
        }

        public void SetSubtitleTrack(int id)
        {
            _mediaPlayer?.SetSpu(id);
        }

        public void Dispose()
        {
            var mediaPlayer = _mediaPlayer;
            var libVlc = _libVlc;
            _mediaPlayer = null;
            _libVlc = null;

            try
            {
                mediaPlayer?.Stop();
            }
            catch
            {
            }

            try
            {
                mediaPlayer?.Dispose();
            }
            catch
            {
            }

            try
            {
                libVlc?.Dispose();
            }
            catch
            {
            }
        }
    }
}
