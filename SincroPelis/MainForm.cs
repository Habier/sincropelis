using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SincroPelis.Properties;
using LibVLCSharp.Shared;

namespace SincroPelis
{
    public partial class MainForm : Form
    {
        private bool _isFullscreen = false;
        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;
#pragma warning disable CS0169, CS0414
        private System.Windows.Forms.Timer? _timerWmpPoll;
        private int _lastWmpState = -1;
#pragma warning restore CS0169, CS0414
        // UI timer used to update position/labels. Kept as field to pause/resume during seeking.
        private System.Windows.Forms.Timer? _uiTimer;
        // Flag set while the user is interacting with the position trackbar to avoid UI glitches.
        private volatile bool _isSeeking = false;
        // When true, the next LibVLC state event (Playing/Paused) will not emit a client message
        private volatile bool _suppressClientEvent = false;
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
        public MainForm()
        {
            InitializeComponent();
            // Initialize LibVLC and VideoView
            try
            {
                LibVLCSharp.Shared.Core.Initialize();
                try
                {
                    _libVLC = new LibVLC(new string[] { "--vout=direct3d11", "--no-video-title-show" });
                }
                catch
                {
                    _libVLC = new LibVLC();
                }
                _mediaPlayer = new MediaPlayer(_libVLC);
                try { videoView.CreateControl(); } catch { }
                videoView.MediaPlayer = _mediaPlayer;
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
                try { Program.client.OnMessageReceived += OnSocketMessageReceived; } catch { }
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
                try { if (_mediaPlayer != null) trackBarVolume.Value = _mediaPlayer.Volume; } catch { }
            }
            catch (Exception ex)
            {
                Logger.Error("LibVLC init failed: " + ex.Message, ex);
                SendDebug("LibVLC init failed: " + ex.Message);
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
                if (_mediaPlayer != null)
                {
                    if (_mediaPlayer.IsPlaying)
                    {
                        // suppress the upcoming Paused event from emitting another client message
                        _suppressClientEvent = true;
                        _mediaPlayer.Pause();
                        SafeInvoke(() => playPauseButton.Text = "Play");
                        Program.client.TrySend("pause");
                    }
                    else
                    {
                        // suppress the upcoming Playing event from emitting another client message
                        _suppressClientEvent = true;
                        _mediaPlayer.Play();
                        SafeInvoke(() => playPauseButton.Text = "Pause");
                        Program.client.TrySend("play");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error en PlayPauseButton_Click", ex);
            }
        }

        private void StopButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _mediaPlayer?.Stop();
                SafeInvoke(() =>
                {
                    playPauseButton.Text = "Play";
                    trackBarPosition.Value = 0;
                    labelTime.Text = "00:00/00:00";
                });
                Program.client.TrySend("stop");
            }
            catch (Exception ex)
            {
                Logger.Error("Error en StopButton_Click", ex);
            }
        }

        private void BackButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    var newTime = Math.Max(0, _mediaPlayer.Time - 5000);
                    _mediaPlayer.Time = (long)newTime;
                    Program.client.TrySend("seekby:-5");
                }
            }
            catch { }
        }

        private void ForwardButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    var newTime = Math.Min(_mediaPlayer.Length, _mediaPlayer.Time + 5000);
                    _mediaPlayer.Time = (long)newTime;
                    Program.client.TrySend("seekby:5");
                }
            }
            catch { }
        }

        private void TrackBarPosition_Scroll(object? sender, EventArgs e)
        {
            try
            {
                if (_mediaPlayer != null && _mediaPlayer.Length > 0)
                {
                    // user requested seek
                    var pos = trackBarPosition.Value / (double)trackBarPosition.Maximum;
                    _mediaPlayer.Position = (float)pos;
                    Program.client.TrySend($"seek:{pos}");
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
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Volume = trackBarVolume.Value;
                }
            }
            catch { }
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_mediaPlayer != null && _mediaPlayer.Length > 0 && !_isSeeking)
                {
                    var position = _mediaPlayer.Position; // 0..1
                    SafeInvoke(() =>
                    {
                        trackBarPosition.Value = Math.Min(trackBarPosition.Maximum, (int)(position * trackBarPosition.Maximum));
                        var current = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
                        var total = TimeSpan.FromMilliseconds(_mediaPlayer.Length);
                        labelTime.Text = string.Format("{0:mm\\:ss}/{1:mm\\:ss}", current, total);
                    });
                }
                // keep _isFullscreen in sync with LibVLC native fullscreen state
                try
                {
                    if (_mediaPlayer != null)
                    {
                        var libFull = _mediaPlayer.Fullscreen;
                        if (libFull != _isFullscreen)
                        {
                            _isFullscreen = libFull;
                            SafeInvoke(() => { try { fullscreenButton.Text = libFull ? "Salir Pantalla" : "Pantalla Completa"; } catch { } });
                        }
                    }
                }
                catch { }
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
                        if (_mediaPlayer != null)
                        {
                            _mediaPlayer.Volume = Math.Min(100, _mediaPlayer.Volume + 5);
                            trackBarVolume.Value = _mediaPlayer.Volume;
                        }
                        break;
                    case KeyController.Shortcut.VolumeDown:
                        if (_mediaPlayer != null)
                        {
                            _mediaPlayer.Volume = Math.Max(0, _mediaPlayer.Volume - 5);
                            trackBarVolume.Value = _mediaPlayer.Volume;
                        }
                        break;
                }
            }
            catch { }
        }

        // Helper to perform safe UI updates from libVLC threads
        private void SafeInvoke(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        // Load audio tracks into combo box AFTER media is loaded
        private void LoadAudioTracks()
        {
            try
            {
                SafeInvoke(() => comboAudio.Items.Clear());
                if (_mediaPlayer == null) return;
                var desc = _mediaPlayer.AudioTrackDescription;
                if (desc == null) return;
                foreach (var d in desc)
                {
                    var id = d.Id;
                    var name = string.IsNullOrEmpty(d.Name) ? $"Track {id}" : d.Name;
                    SafeInvoke(() => comboAudio.Items.Add(new ComboItem(name, id)));
                }
                // select current
                var current = _mediaPlayer.AudioTrack;
                SafeInvoke(() =>
                {
                    for (int i = 0; i < comboAudio.Items.Count; i++)
                    {
                        if (comboAudio.Items[i] is ComboItem ci && ci.Id == current)
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
                if (_mediaPlayer == null) return;
                // add None
                SafeInvoke(() => comboSub.Items.Add(new ComboItem("None", -1)));
                var desc = _mediaPlayer.SpuDescription;
                if (desc == null) return;
                foreach (var d in desc)
                {
                    var id = d.Id;
                    var name = string.IsNullOrEmpty(d.Name) ? $"Sub {id}" : d.Name;
                    SafeInvoke(() => comboSub.Items.Add(new ComboItem(name, id)));
                }
                // select current
                var current = _mediaPlayer.Spu;
                SafeInvoke(() =>
                {
                    for (int i = 0; i < comboSub.Items.Count; i++)
                    {
                        if (comboSub.Items[i] is ComboItem ci && ci.Id == current)
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
                _suppressClientEvent = true;

                if (message == "pause")
                {
                    _mediaPlayer?.Pause();
                    SafeInvoke(() => playPauseButton.Text = "Pause");
                }
                else if (message == "play")
                {
                    _mediaPlayer?.Play();
                    SafeInvoke(() => playPauseButton.Text = "Play");
                }
                else if (message == "stop")
                {
                    _mediaPlayer?.Stop();
                    SafeInvoke(() => { playPauseButton.Text = "Play"; trackBarPosition.Value = 0; });
                }
                else if (message == "ended")
                {
                    SafeInvoke(() => { playPauseButton.Text = "Play"; trackBarPosition.Value = 0; });
                }
                else if (message.StartsWith("seek:"))
                {
                    var posStr = message.Substring(5);
                    if (double.TryParse(posStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double pos))
                    {
                        if (_mediaPlayer != null && _mediaPlayer.Length > 0)
                        {
                            _mediaPlayer.Position = (float)pos;
                        }
                    }
                }
                else if (message.StartsWith("seekby:"))
                {
                    var secStr = message.Substring(7);
                    if (int.TryParse(secStr, out int sec) && _mediaPlayer != null)
                    {
                        var newTime = Math.Max(0, Math.Min(_mediaPlayer.Length, _mediaPlayer.Time + sec * 1000));
                        _mediaPlayer.Time = newTime;
                    }
                }


                else if (message.StartsWith("audio:"))
                {
                    var idStr = message.Substring(6);
                    if (int.TryParse(idStr, out int id) && _mediaPlayer != null)
                    {
                        _mediaPlayer.SetAudioTrack(id);
                    }
                }
                else if (message.StartsWith("sub:"))
                {
                    var idStr = message.Substring(4);
                    if (int.TryParse(idStr, out int id) && _mediaPlayer != null)
                    {
                        _mediaPlayer.SetSpu(id);
                    }
                }

                SendDebug("Socket: " + message);
            }
            catch { }
            finally
            {
                _suppressClientEvent = false;
            }
        }

        private void selfConnect()
        {
            Program.server.start();
            Program.client.TryConnect("127.0.0.1");
        }

        private void masterMode()
        {
            Program.serverTask = new Task(selfConnect);
            Program.serverTask.Start();
        }

        private void clientMode()
        {
            Program.client.TryConnect(textHost.Text);
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

            Client.PORT = number;
            Server.PORT = number;
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
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
                LoadFileToPlayer(path, false); // open and don't force pause
            }
            catch (Exception ex)
            {
                SendDebug("Error cargando fichero: " + ex.Message);
            }
        }

        private void LoadFileToPlayer(string path, bool startPaused)
        {
            if (_mediaPlayer != null && _libVLC != null)
            {
                try
                {
                    using var media = new LibVLCSharp.Shared.Media(_libVLC, path, LibVLCSharp.Shared.FromType.FromPath);
                    _mediaPlayer.Play(media);
                    if (startPaused)
                    {
                        try { _mediaPlayer.Pause(); } catch { }
                    }
                    SendDebug(startPaused ? "Fichero cargado en LibVLC (pausado)." : "Reproduciendo en reproductor integrado (LibVLC).");
                    return;
                }
                catch (Exception ex)
                {
                    SendDebug("Error cargando en LibVLC: " + ex.Message);
                }
            }

            SendDebug("No hay reproductor disponible: ");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            try
            {
                _mediaPlayer?.Stop();
                _mediaPlayer?.Dispose();
                _libVLC?.Dispose();
            }
            catch { }
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
            if (_mediaPlayer == null) return;
            _mediaPlayer.Playing += (_, _) =>
            {
                try
                {
                    // If this playing event was triggered by a local action where we already sent the
                    // client message, suppress sending again. Reset the flag either way.
                    if (_suppressClientEvent)
                    {
                        _suppressClientEvent = false;
                    }
                    else
                    {
                        Program.client.TrySend("play");
                    }

                    SendDebug("LibVLC: Playing");
                    SafeInvoke(() => { playPauseButton.Text = "Pause"; SetControlsEnabled(true); });
                }
                catch { }
            };

            _mediaPlayer.Paused += (_, _) =>
            {
                try
                {
                    if (_suppressClientEvent)
                    {
                        _suppressClientEvent = false;
                    }
                    else
                    {
                        Program.client.TrySend("pause");
                    }

                    SendDebug("LibVLC: Paused");
                    SafeInvoke(() => playPauseButton.Text = "Play");
                }
                catch { }
            };

            _mediaPlayer.Stopped += (_, _) => { try { Program.client.TrySend("stop"); SendDebug("LibVLC: Stopped"); SafeInvoke(() => { playPauseButton.Text = "Play"; SetControlsEnabled(false); }); } catch { } };
            _mediaPlayer.EndReached += (_, _) => { try { Program.client.TrySend("ended"); SendDebug("LibVLC: Ended"); SafeInvoke(() => { playPauseButton.Text = "Play"; trackBarPosition.Value = 0; SetControlsEnabled(false); }); } catch { } };
            _mediaPlayer.LengthChanged += (_, _) => { try { SafeInvoke(() => UiTimer_Tick(null, EventArgs.Empty)); } catch { } };

            // Ensure audio/subtitle lists are loaded when media becomes ready
            _mediaPlayer.Playing += (_, _) => { try { LoadAudioTracks(); LoadSubtitleTracks(); } catch { } };
        }

        // Simple container to store combo display and ID
        private class ComboItem
        {
            public string Name { get; }
            public int Id { get; }
            public ComboItem(string name, int id) { Name = name; Id = id; }
            public override string ToString() => Name;
        }

        // Event handlers for audio/subtitle combo boxes (wired in designer)
        private void comboAudio_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_mediaPlayer == null) return;
                if (comboAudio.SelectedItem is ComboItem it)
                {
                    _mediaPlayer.SetAudioTrack(it.Id);
                    Program.client.TrySend($"audio:{it.Id}");
                }
            }
            catch { }
        }

        private void comboSub_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_mediaPlayer == null) return;
                if (comboSub.SelectedItem is ComboItem it)
                {
                    // id -1 disables
                    _mediaPlayer.SetSpu(it.Id);
                    Program.client.TrySend($"sub:{it.Id}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error en comboSub_SelectedIndexChanged", ex);
            }
        }
    }
}
