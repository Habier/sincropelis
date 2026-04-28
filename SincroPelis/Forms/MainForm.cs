using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using SincroPelis.Playback;
using SincroPelis.Properties;

namespace SincroPelis
{
    public partial class MainForm : Form
    {
        private readonly Client _client;
        private readonly Server _server;
        private readonly PlaybackService _playbackService;
        private bool _isFullscreen = false;
#pragma warning disable CS0169, CS0414
        private System.Windows.Forms.Timer? _timerWmpPoll;
        private int _lastWmpState = -1;
#pragma warning restore CS0169, CS0414
        // UI timer used to update position/labels. Kept as field to pause/resume during seeking.
        private System.Windows.Forms.Timer? _uiTimer;
        // Flag set while the user is interacting with the position trackbar to avoid UI glitches.
        private volatile bool _isSeeking = false;
        // Guard against LibVLC callbacks/timers touching disposed native objects while closing.
        private volatile bool _isClosing = false;
        // Enable/disable playback-related controls safely
        private void SetControlsEnabled(bool enabled)
        {
            SafeInvoke(() =>
            {
                playPauseButton.Enabled = enabled;
                backButton.Enabled = enabled;
                forwardButton.Enabled = enabled;
                trackBarPosition.Enabled = enabled;
                trackBarVolume.Enabled = enabled;
                comboAudio.Enabled = enabled;
                comboSub.Enabled = enabled;
            });
        }
        public MainForm(Client client, Server server)
        {
            _client = client;
            _server = server;
            _playbackService = new PlaybackService();

            InitializeComponent();

            string? systemVLCLibPath = VLCDownloader.GetSystemVLCLibPath();
            string? vlcDownloadedPath = null;

            if (!string.IsNullOrEmpty(systemVLCLibPath))
            {
                Environment.SetEnvironmentVariable("LIBVLC_PATH", systemVLCLibPath);
                Environment.SetEnvironmentVariable("LIBVLC_PLUGINS", Path.Combine(systemVLCLibPath, "plugins"));
            }

            if (!VLCDownloader.IsVLCAvailable())
            {
                var result = MessageBox.Show(
                    "VLC no encontrado. ¿Deseas descargarlo automáticamente?",
                    "VLC requerido",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using var downloadForm = new VLCDownloadForm();
                    if (downloadForm.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("VLC es requerido para reproducir video.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                        return;
                    }
                    vlcDownloadedPath = VLCDownloader.GetVLCLibPath();
                }
                else
                {
                    MessageBox.Show("VLC es requerido para reproducir video.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
            }

            try
            {
                _playbackService.Initialize(systemVLCLibPath, vlcDownloadedPath);
                try { videoView.CreateControl(); } catch { }
                videoView.MediaPlayer = _playbackService.MediaPlayer;
                videoView.Visible = true;
                webBrowserPlayer.Visible = false;
                // start with controls disabled until media is ready
                SetControlsEnabled(false);
                WireMediaPlayerEvents();
                // mouse click: single click = play/pause, double click = fullscreen
                try { videoView.Click += (s, e) => PlayPauseButton_Click(null, EventArgs.Empty); } catch { }
                // allow form to receive key events (for ESC to exit fullscreen)
                try { this.KeyPreview = true; } catch { }
                try { KeyController.OnShortcutPressed += OnShortcutPressed; } catch { }
                try { this.FormClosing += Form1_FormClosing; } catch { }
                // wire control events (designer event hookups were removed; subscribe at runtime)
                try { playPauseButton.Click += PlayPauseButton_Click; } catch { }
                try { backButton.Click += BackButton_Click; } catch { }
                try { forwardButton.Click += ForwardButton_Click; } catch { }
                try { trackBarPosition.Scroll += TrackBarPosition_Scroll; } catch { }
                try { trackBarPosition.MouseDown += TrackBarPosition_MouseDown; } catch { }
                try { trackBarPosition.MouseUp += TrackBarPosition_MouseUp; } catch { }
                try { trackBarVolume.Scroll += TrackBarVolume_Scroll; } catch { }
                try { comboAudio.SelectedIndexChanged += comboAudio_SelectedIndexChanged; } catch { }
                try { comboSub.SelectedIndexChanged += comboSub_SelectedIndexChanged; } catch { }
                // other UI events (were previously wired in designer)
                try { fullscreenButton.Click += (s, e) => fullscreenButton_Click(this, e); } catch { }
                try { buttonConnect.Click += (s, e) => buttonConnect_Click(this, e); } catch { }
                try { _client.OnMessageReceived += OnSocketMessageReceived; } catch { }
                try { textHost.TextChanged += (s, e) => textHost_TextChanged(this, e); } catch { }

                try { checkBoxMaestro.CheckedChanged += (s, e) => checkBoxMaestro_CheckedChanged(this, e); } catch { }
                try { textBoxPort.TextChanged += (s, e) => textBoxPort_TextChanged(this, e); } catch { }
                try { textBoxPort.KeyPress += (s, e) => textBoxPort_KeyPress(this, e); } catch { }
                try { buttonSelectFile.Click += (s, e) => buttonSelectFile_Click(this, e); } catch { }
                try { textBoxFilePath.TextChanged += (s, e) => textBoxFilePath_TextChanged(this, e); } catch { }
                try { videoView.Enter += (s, e) => videoView_Enter(this, e); } catch { }
                try
                {
                    if (_uiTimer == null)
                    {
                        _uiTimer = new System.Windows.Forms.Timer();
                        _uiTimer.Interval = 500;
                        _uiTimer.Tick += UiTimer_Tick;
                        _uiTimer.Start();
                    }
                }
                catch { }
                // initialize volume control from player
                try { trackBarVolume.Value = _playbackService.GetVolume(); } catch { }
            }
            catch (Exception ex)
            {
                Logger.Error($"LibVLC initialization failed: {ex.Message}", ex);
                SendDebug($"Error al inicializar LibVLC: {ex.Message}");
            }
        }

        // no WMP host anymore

        // Timer poll removed (was used for WMP)

        // LibVLC removed — WMP (axWindowsMediaPlayer1) is primary player now.

        private void Form1_Load(object sender, EventArgs e)
        {
            textHost.Text = Settings.Default.host;

            textBoxPort.Text = Settings.Default.port.ToString();
        }

        private void PlayPauseButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_playbackService.TryGetActiveMediaPlayer(out _))
                {
                    if (_playbackService.TogglePlayPause())
                    {
                        SafeInvoke(() => playPauseButton.Text = "Pause");
                        _client.TrySend("play");
                    }
                    else
                    {
                        SafeInvoke(() => playPauseButton.Text = "Play");
                        _client.TrySend("pause");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in PlayPauseButton_Click handler", ex);
            }
        }

        private void StopButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _playbackService.Stop();
                SafeInvoke(() =>
                {
                    playPauseButton.Text = "Play";
                    trackBarPosition.Value = 0;
                    labelTime.Text = "00:00/00:00";
                });
                _client.TrySend("stop");
            }
            catch (Exception ex)
            {
                Logger.Error("Error in StopButton_Click handler", ex);
            }
        }

        private void BackButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_playbackService.TryGetActiveMediaPlayer(out _))
                {
                    _playbackService.SeekBySeconds(-5);
                    _client.TrySend("seekby:-5");
                }
            }
            catch { }
        }

        private void ForwardButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (TryGetActiveMediaPlayer(out var mediaPlayer))
                {
                    _playbackService.SeekBySeconds(5);
                    _client.TrySend("seekby:5");
                }
            }
            catch { }
        }

        private void TrackBarPosition_Scroll(object? sender, EventArgs e)
        {
            try
            {
                if (TryGetActiveMediaPlayer(out var mediaPlayer) && mediaPlayer.Length > 0)
                {
                    // user requested seek
                    var pos = trackBarPosition.Value / (double)trackBarPosition.Maximum;
                    _playbackService.SeekToPosition(pos);
                    _client.TrySend($"seek:{pos}");
                }
            }
            catch { }
        }

        private void TrackBarPosition_MouseDown(object? sender, MouseEventArgs e)
        {
            _isSeeking = true;
        }

        private void TrackBarPosition_MouseUp(object? sender, MouseEventArgs e)
        {
            _isSeeking = false;
        }

        private void TrackBarVolume_Scroll(object? sender, EventArgs e)
        {
            try
            {
                _playbackService.SetVolume(trackBarVolume.Value);
            }
            catch { }
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (TryGetActiveMediaPlayer(out var mediaPlayer) && mediaPlayer.Length > 0 && !_isSeeking)
                {
                    var position = mediaPlayer.Position; // 0..1
                    var current = TimeSpan.FromMilliseconds(mediaPlayer.Time);
                    var total = TimeSpan.FromMilliseconds(mediaPlayer.Length);
                    SafeInvoke(() =>
                    {
                        trackBarPosition.Value = Math.Min(trackBarPosition.Maximum, (int)(position * trackBarPosition.Maximum));
                        labelTime.Text = string.Format("{0:mm\\:ss}/{1:mm\\:ss}", current, total);
                    });
                }
                // keep _isFullscreen in sync with LibVLC native fullscreen state
                try
                {
                    if (TryGetActiveMediaPlayer(out mediaPlayer))
                    {
                        var libFull = mediaPlayer.Fullscreen;
                        if (libFull != _isFullscreen)
                        {
                            _isFullscreen = libFull;
                            SafeInvoke(() => { try { fullscreenButton.Text = libFull ? "Salir Pantalla" : "Pantalla Completa"; } catch { } });
                        }
                    }
                }
                catch { }
                // update connected users count if in server mode
                if (checkBoxMaestro.Checked)
                {
                    SafeInvoke(() => labelConnectedUsers.Text = $"Conectados: {_server.ConnectedClients}");
                }
            }
            catch { }
        }

        private void OnShortcutPressed(KeyController.Shortcut shortcut)
        {
            try
            {
                switch (shortcut)
                {
                    case KeyController.Shortcut.PlayPause:
                        PlayPauseButton_Click(null, EventArgs.Empty);
                        break;
                    case KeyController.Shortcut.SeekBack:
                        BackButton_Click(null, EventArgs.Empty);
                        break;
                    case KeyController.Shortcut.SeekForward:
                        ForwardButton_Click(null, EventArgs.Empty);
                        break;
                    case KeyController.Shortcut.Escape:
                        if (_isFullscreen || _fsForm != null)
                            ToggleFullscreen();
                        break;
                    case KeyController.Shortcut.VolumeUp:
                        if (_playbackService.TryGetActiveMediaPlayer(out _))
                        {
                            trackBarVolume.Value = _playbackService.AdjustVolume(5);
                        }
                        break;
                    case KeyController.Shortcut.VolumeDown:
                        if (_playbackService.TryGetActiveMediaPlayer(out _))
                        {
                            trackBarVolume.Value = _playbackService.AdjustVolume(-5);
                        }
                        break;
                }
            }
            catch { }
        }

        // Helper to perform safe UI updates from libVLC threads
        private void SafeInvoke(Action action)
        {
            if (_isClosing || IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private bool TryGetActiveMediaPlayer(out MediaPlayer mediaPlayer)
        {
            mediaPlayer = null!;

            if (_isClosing || IsDisposed)
            {
                return false;
            }

            if (!_playbackService.TryGetActiveMediaPlayer(out var current))
            {
                return false;
            }

            mediaPlayer = current;
            return true;
        }

        // Load audio tracks into combo box AFTER media is loaded
        private void LoadAudioTracks()
        {
            try
            {
                SafeInvoke(() => comboAudio.Items.Clear());
                var audioTracks = _playbackService.GetAudioTracks();
                foreach (var track in audioTracks)
                {
                    SafeInvoke(() => comboAudio.Items.Add(track));
                }

                var current = _playbackService.GetCurrentAudioTrack();
                SafeInvoke(() =>
                {
                    for (int i = 0; i < comboAudio.Items.Count; i++)
                    {
                        if (comboAudio.Items[i] is TrackOption ci && ci.Id == current)
                        {
                            comboAudio.SelectedIndex = i;
                            break;
                        }
                    }
                });
            }
            catch { }
        }

        // Load subtitle (SPU) tracks into combo box; include "None" option
        private void LoadSubtitleTracks()
        {
            try
            {
                SafeInvoke(() => comboSub.Items.Clear());
                var subtitleTracks = _playbackService.GetSubtitleTracks();
                foreach (var track in subtitleTracks)
                {
                    SafeInvoke(() => comboSub.Items.Add(track));
                }

                var current = _playbackService.GetCurrentSubtitleTrack();
                SafeInvoke(() =>
                {
                    for (int i = 0; i < comboSub.Items.Count; i++)
                    {
                        if (comboSub.Items[i] is TrackOption ci && ci.Id == current)
                        {
                            comboSub.SelectedIndex = i;
                            break;
                        }
                    }
                });
            }
            catch { }
        }



        public void SendDebug(string text)
        {
            labelDebug.Invoke(new Action(() => labelDebug.Text = text));
        }



        private void fullscreenButton_Click(object sender, EventArgs e)
        {
            SendDebug("fullscreenButton clicked");
            ToggleFullscreen();
        }

        private FullscreenForm? _fsForm = null;

        private void ToggleFullscreen()
        {
            SendDebug("ToggleFullscreen invoked");
            try
            {
                if (_fsForm == null)
                {
                    _fsForm = new FullscreenForm(videoView, () =>
                    {
                        _fsForm = null;
                        _isFullscreen = false;
                        SafeInvoke(() => fullscreenButton.Text = "Pantalla Completa");
                    });
                    _fsForm.Show();
                    _isFullscreen = true;
                    SafeInvoke(() => fullscreenButton.Text = "Salir Pantalla");
                    SendDebug("Fullscreen opened");
                }
                else
                {
                    _fsForm.Close();
                    _fsForm = null;
                }
            }
            catch (Exception ex)
            {
                SendDebug("ToggleFullscreen exception: " + ex.Message);
            }
        }

        private void OnSocketMessageReceived(string message)
        {
            try
            {
                var result = _playbackService.HandleIncomingCommand(message);

                if (result.PlayPauseText != null)
                {
                    SafeInvoke(() => playPauseButton.Text = result.PlayPauseText);
                }

                if (result.ResetPosition)
                {
                    SafeInvoke(() => trackBarPosition.Value = 0);
                }

                SendDebug("Socket: " + message);
            }
            catch { }
        }

        private void selfConnect()
        {
            _server.start();
            _client.TryConnect("127.0.0.1");
        }

        private void masterMode()
        {
            Task.Run(selfConnect);
        }

        private void clientMode()
        {
            _client.TryConnect(textHost.Text);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (checkBoxMaestro.Checked)
            {
                masterMode();
            }
            else
            {
                clientMode();
            }
        }

        private void textHost_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.host = textHost.Text;
            Settings.Default.Save();
        }


        private void textBoxPort_TextChanged(object sender, EventArgs e)
        {
            int number = 9000; //Default port;

            if (!String.IsNullOrEmpty(textBoxPort.Text))
                number = Convert.ToInt32(textBoxPort.Text);

            Settings.Default.port = number;
            Settings.Default.Save();

            _client.Port = number;
            _server.Port = number;
        }



        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            const string message = "¿Desea a ciencia cierta salir de esta aplicación?";
            const string caption = "¿Seguuuuro?";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            e.Cancel = (result == DialogResult.No);
        }

        private void checkBoxMaestro_CheckedChanged(object sender, EventArgs e)
        {
            textHost.Enabled = !checkBoxMaestro.Checked;
            labelConnectedUsers.Visible = checkBoxMaestro.Checked;
            if (!checkBoxMaestro.Checked)
            {
                labelConnectedUsers.Text = "";
            }
        }

        private void textBoxPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv|All Files|*.*";
                ofd.Title = "Seleccionar fichero de video";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBoxFilePath.Text = ofd.FileName;
                    // Open immediately in player, paused
                    try { LoadFileToPlayer(ofd.FileName, true); } catch { }
                }
            }
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            var path = textBoxFilePath.Text;
            if (String.IsNullOrEmpty(path))
            {
                SendDebug("No hay fichero seleccionado.");
                return;
            }

            try
            {
                LoadFileToPlayer(path, true); // open paused
            }
            catch (Exception ex)
            {
                SendDebug("Error cargando fichero: " + ex.Message);
            }
        }

        private void LoadFileToPlayer(string path, bool startPaused)
        {
            _playbackService.LoadFile(path, startPaused, SendDebug);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isClosing = true;
            base.OnFormClosing(e);

            try
            {
                _uiTimer?.Stop();
                _uiTimer?.Dispose();
                _uiTimer = null;
            }
            catch { }

            try
            {
                if (_fsForm != null && !_fsForm.IsDisposed)
                {
                    _fsForm.Close();
                }
            }
            catch { }

            try
            {
                videoView.MediaPlayer = null;
            }
            catch { }

            _playbackService.Dispose();
        }

        private void textBoxFilePath_TextChanged(object sender, EventArgs e)
        {

        }

        private void videoView_Enter(object sender, EventArgs e)
        {

        }

        // Map LibVLC events to client messages
        private void WireMediaPlayerEvents()
        {
            if (!_playbackService.TryGetActiveMediaPlayer(out var mediaPlayer)) return;
            mediaPlayer.Playing += (_, _) =>
            {
                try
                {
                    if (_playbackService.ConsumeSuppressedSyncEvent())
                    {
                    }
                    else
                    {
                        _client.TrySend("play");
                    }

                    SendDebug("LibVLC: Playing");
                    SafeInvoke(() => { playPauseButton.Text = "Pause"; SetControlsEnabled(true); });
                }
                catch { }
            };

            mediaPlayer.Paused += (_, _) =>
            {
                try
                {
                    if (_playbackService.ConsumeSuppressedSyncEvent())
                    {
                    }
                    else
                    {
                        _client.TrySend("pause");
                    }

                    SendDebug("LibVLC: Paused");
                    SafeInvoke(() => playPauseButton.Text = "Play");
                }
                catch { }
            };

            mediaPlayer.Stopped += (_, _) => { try { _client.TrySend("stop"); SendDebug("LibVLC: Stopped"); SafeInvoke(() => { playPauseButton.Text = "Play"; SetControlsEnabled(false); }); } catch { } };
            mediaPlayer.EndReached += (_, _) => { try { _client.TrySend("ended"); SendDebug("LibVLC: Ended"); SafeInvoke(() => { playPauseButton.Text = "Play"; trackBarPosition.Value = 0; SetControlsEnabled(false); }); } catch { } };
            mediaPlayer.LengthChanged += (_, _) => { try { SafeInvoke(() => UiTimer_Tick(null, EventArgs.Empty)); } catch { } };

            // Ensure audio/subtitle lists are loaded when media becomes ready
            mediaPlayer.Playing += (_, _) => { try { LoadAudioTracks(); LoadSubtitleTracks(); } catch { } };
        }

        // Event handlers for audio/subtitle combo boxes (wired in designer)
        private void comboAudio_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (comboAudio.SelectedItem is TrackOption it)
                {
                    _playbackService.SetAudioTrack(it.Id);
                    _client.TrySend($"audio:{it.Id}");
                }
            }
            catch { }
        }

        private void comboSub_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (comboSub.SelectedItem is TrackOption it)
                {
                    _playbackService.SetSubtitleTrack(it.Id);
                    _client.TrySend($"sub:{it.Id}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in comboSub_SelectedIndexChanged handler", ex);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
