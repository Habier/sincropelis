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
        private TaskCompletionSource<bool>? _tcs;
        private bool _downloadCompleted = false;

        public VLCDownloadForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(400, 180);
            this.Text = "Descargando VLC";
            this.Load += VLCDownloadForm_Load;
        }

        private void InitializeComponent()
        {
            labelTitle = new Label
            {
                Text = "VLC no encontrado. ¿Deseas descargarlo?",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            labelStatus = new Label
            {
                Text = "",
                Location = new Point(20, 100),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 130),
                Size = new Size(360, 25),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            buttonCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(150, 60),
                Size = new Size(100, 30)
            };
            buttonCancel.Click += ButtonCancel_Click;
        }

        private async void VLCDownloadForm_Load(object sender, EventArgs e)
        {
            _tcs = new TaskCompletionSource<bool>();

            var progress = new Progress<string>(msg =>
            {
                labelStatus.Text = msg;
                if (msg.Contains("%"))
                {
                    progressBar.Visible = true;
                    var parts = msg.Split('(');
                    if (parts.Length > 1 && parts[1].Contains("%"))
                    {
                        var percentStr = parts[1].Split('%')[0];
                        if (int.TryParse(percentStr, out int percent))
                            progressBar.Value = Math.Min(100, percent);
                    }
                }
            });

            var success = await VLCDownloader.DownloadAndExtractAsync(progress);

            _downloadCompleted = success;

            if (success)
            {
                labelStatus.Text = "¡VLC descargado! Iniciando...";
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                labelStatus.Text = "Error al descargar VLC. Puedes descargarlo manualmente desde videolan.org";
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