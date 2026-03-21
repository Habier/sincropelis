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
        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;

        public MainForm()
        {
            InitializeComponent();
            Core.Initialize();

            timerVlcStatus.Interval = 500;
            timerVlcStatus.Tick += TimerVlcStatus_Tick;

            try
            {
                // Prefer Direct3D11 video output on Windows to avoid black video surfaces
                try
                {
                    _libVLC = new LibVLC(new string[] { "--vout=direct3d11", "--no-video-title-show" });
                }
                catch
                {
                    // fallback to default if option unsupported
                    _libVLC = new LibVLC();
                }

                _mediaPlayer = new MediaPlayer(_libVLC);
                // Ensure the VideoView has a handle created before assigning MediaPlayer
                try { videoView.CreateControl(); } catch { }
                videoView.MediaPlayer = _mediaPlayer;
                videoView.Visible = true;
                webBrowserPlayer.Visible = false;
            }
            catch (Exception ex)
            {
                SendDebug("LibVLC init failed: " + ex.Message);
                // keep webBrowser fallback
                webBrowserPlayer.Visible = true;
                videoView.Visible = false;
            }
        }

        private void buttonReinitVlc_Click(object sender, EventArgs e)
        {
            // Keep for compatibility — just reinit automatically
            try
            {
                timerVlcStatus.Stop();
                _mediaPlayer?.Stop();
                _mediaPlayer?.Dispose();
                _libVLC?.Dispose();
            }
            catch { }

            try
            {
                _libVLC = new LibVLC(new string[] { "--no-video-title-show" });
                _mediaPlayer = new MediaPlayer(_libVLC);
                _mediaPlayer.EncounteredError += (_, _) => SendDebug("MediaPlayer encountered an error.");
                _mediaPlayer.Playing += (_, _) => SendDebug("MediaPlayer playing.");
                try { videoView.CreateControl(); } catch { }
                videoView.MediaPlayer = _mediaPlayer;
                videoView.Visible = true;
                webBrowserPlayer.Visible = false;
                SendDebug("LibVLC reiniciado (automatic).");
            }
            catch (Exception ex)
            {
                SendDebug("Reinicio de LibVLC fallido: " + ex.Message);
            }
        }

        private void TimerVlcStatus_Tick(object? sender, EventArgs e)
        {
            // if media is playing but surface size is zero, log it
            try
            {
                if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
                {
                    // Query video size via video track dimensions
                    try
                    {
                        uint w = 0, h = 0;
                        _mediaPlayer.Size(0u, ref w, ref h);
                        if (w == 0 || h == 0)
                        {
                            SendDebug($"Video surface size {w}x{h} — posible problema de render.");
                        }
                        else
                        {
                            // OK
                            timerVlcStatus.Stop();
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textHost.Text = Settings.Default.host;
            pName.Text = Settings.Default.videoplayer;
            textBoxPort.Text = Settings.Default.port.ToString();
        }



        public void SendDebug(string text)
        {
            labelDebug.Invoke(new Action(() => labelDebug.Text = text));
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            //Cheats.SendKey2Process(pName.Text);
            Program.client.TrySend();
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

        private void pName_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.videoplayer = pName.Text;
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

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            var playerName = PlayerFinder.find();
            if (playerName != String.Empty)
                pName.Text = playerName;
        }

        public string getProcessName()
        {
            return pName.Text;
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
                if (_mediaPlayer != null && _libVLC != null)
                {
                    // use libVLC to play the file
                    videoView.BackColor = Color.Black;
                    videoView.Visible = true;
                    videoView.BringToFront();
                    webBrowserPlayer.Visible = false;
                    // ensure media player attached
                    if (videoView.MediaPlayer == null) videoView.MediaPlayer = _mediaPlayer;
                    try { videoView.CreateControl(); } catch { }
                    videoView.Visible = true;
                    videoView.BringToFront();
                    videoView.Invalidate();
                    Application.DoEvents();
                    // small delay to allow native surface creation
                    try { Thread.Sleep(150); } catch { }

                    using var media = new Media(_libVLC, path, FromType.FromPath);
                    var playResult = _mediaPlayer.Play(media);
                    timerVlcStatus.Start();
                    SendDebug("Reproduciendo en reproductor integrado (LibVLC). Play result=" + playResult.ToString());
                }
                else
                {
                    // Load the video into the embedded WebBrowser using an HTML5 video tag
                    var uri = new Uri(path);
                    var html = $"<html><body style=\"margin:0;background:black\"><video width=\"100%\" height=\"100%\" controls autoplay src=\"{uri.AbsoluteUri}\">Your browser does not support the video tag.</video></body></html>";
                    webBrowserPlayer.DocumentText = html;
                    webBrowserPlayer.Visible = true;
                    videoView.Visible = false;
                    SendDebug("Fichero cargado en reproductor integrado (WebBrowser).");
                }
            }
            catch (Exception ex)
            {
                // Fallback: try to open the file with default associated app
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                    SendDebug("Fichero abierto con la aplicación por defecto.");
                }
                catch (Exception inner)
                {
                    SendDebug("Error abriendo el fichero: " + inner.Message);
                }
            }
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
    }
}
