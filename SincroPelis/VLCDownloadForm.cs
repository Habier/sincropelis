using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SincroPelis
{
    public class VLCDownloadForm : Form
    {
        private Label labelTitle;
        private Label labelStatus;
        private ProgressBar progressBar;
        private Button buttonCancel;
        private bool _downloadCompleted = false;

        public VLCDownloadForm()
        {
            InitializeComponent();
            this.Load += VLCDownloadForm_Load;
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(400, 180);
            this.Text = "Descargando VLC";

            labelTitle = new Label
            {
                Text = "Descargando VLC...",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            labelStatus = new Label
            {
                Text = "Preparando...",
                Location = new Point(20, 60),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 95),
                Size = new Size(360, 25),
                Style = ProgressBarStyle.Continuous
            };

            buttonCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(150, 135),
                Size = new Size(100, 30)
            };
            buttonCancel.Click += ButtonCancel_Click;

            this.Controls.Add(labelTitle);
            this.Controls.Add(labelStatus);
            this.Controls.Add(progressBar);
            this.Controls.Add(buttonCancel);
        }

        private async void VLCDownloadForm_Load(object sender, EventArgs e)
        {
            var progress = new Progress<string>(msg =>
            {
                labelStatus.Text = msg;
                if (msg.Contains("%"))
                {
                    var lastSpace = msg.LastIndexOf(' ');
                    if (lastSpace >= 0)
                    {
                        var percentStr = msg.Substring(lastSpace + 1).Replace("%", "");
                        if (int.TryParse(percentStr, out int percent))
                            progressBar.Value = Math.Min(100, percent);
                    }
                }
            });

            try
            {
                var success = await VLCDownloader.DownloadAndExtractAsync(progress);
                _downloadCompleted = success;

                if (success)
                {
                    labelStatus.Text = "¡VLC descargado!";
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    labelStatus.Text = "Error al descargar VLC.";
                    buttonCancel.Text = "Cerrar";
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Error: {ex.Message}";
                buttonCancel.Text = "Cerrar";
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public bool DownloadCompleted => _downloadCompleted;
    }
}