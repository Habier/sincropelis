namespace SincroPelis
{
    partial class MainForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                try { videoView?.Dispose(); } catch { }
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            pauseButton = new Button();
            pName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            textHost = new TextBox();
            buttonConnect = new Button();
            labelDebug = new Label();
            buttonSearch = new Button();
            checkBoxMaestro = new CheckBox();
            textBoxPort = new TextBox();
            label3 = new Label();
            textBoxFilePath = new TextBox();
            buttonSelectFile = new Button();
            buttonOpenFile = new Button();
            webBrowserPlayer = new WebBrowser();
            videoView = new LibVLCSharp.WinForms.VideoView();
            timerVlcStatus = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)videoView).BeginInit();
            SuspendLayout();
            // 
            // pauseButton
            // 
            pauseButton.Location = new Point(313, 417);
            pauseButton.Margin = new Padding(4, 3, 4, 3);
            pauseButton.Name = "pauseButton";
            pauseButton.Size = new Size(316, 58);
            pauseButton.TabIndex = 0;
            pauseButton.Text = "Pausar/Reanudar";
            pauseButton.UseVisualStyleBackColor = true;
            pauseButton.Click += pauseButton_Click;
            // 
            // pName
            // 
            pName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pName.Location = new Point(803, 14);
            pName.Margin = new Padding(4, 3, 4, 3);
            pName.Name = "pName";
            pName.Size = new Size(116, 23);
            pName.TabIndex = 1;
            pName.Text = "mpc-hc64";
            pName.TextChanged += pName_TextChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(582, 17);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(139, 15);
            label1.TabIndex = 2;
            label1.Text = "Nombre del Reproductor";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 17);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 4;
            label2.Text = "Host:";
            // 
            // textHost
            // 
            textHost.Location = new Point(59, 14);
            textHost.Margin = new Padding(4, 3, 4, 3);
            textHost.Name = "textHost";
            textHost.Size = new Size(116, 23);
            textHost.TabIndex = 3;
            textHost.TextChanged += textHost_TextChanged;
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(294, 8);
            buttonConnect.Margin = new Padding(4, 3, 4, 3);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(112, 33);
            buttonConnect.TabIndex = 5;
            buttonConnect.Text = "Conectar";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;
            // 
            // labelDebug
            // 
            labelDebug.AutoSize = true;
            labelDebug.Dock = DockStyle.Bottom;
            labelDebug.ForeColor = Color.Maroon;
            labelDebug.Location = new Point(0, 504);
            labelDebug.Margin = new Padding(4, 0, 4, 0);
            labelDebug.Name = "labelDebug";
            labelDebug.Size = new Size(0, 15);
            labelDebug.TabIndex = 6;
            // 
            // buttonSearch
            // 
            buttonSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSearch.Location = new Point(803, 45);
            buttonSearch.Margin = new Padding(4, 3, 4, 3);
            buttonSearch.Name = "buttonSearch";
            buttonSearch.Size = new Size(117, 29);
            buttonSearch.TabIndex = 7;
            buttonSearch.Text = "Auto buscar";
            buttonSearch.UseVisualStyleBackColor = true;
            buttonSearch.Click += buttonSearch_Click;
            // 
            // checkBoxMaestro
            // 
            checkBoxMaestro.AutoSize = true;
            checkBoxMaestro.Location = new Point(19, 54);
            checkBoxMaestro.Margin = new Padding(4, 3, 4, 3);
            checkBoxMaestro.Name = "checkBoxMaestro";
            checkBoxMaestro.Size = new Size(153, 19);
            checkBoxMaestro.TabIndex = 9;
            checkBoxMaestro.Text = "Conectar como servidor";
            checkBoxMaestro.UseVisualStyleBackColor = true;
            checkBoxMaestro.CheckedChanged += checkBoxMaestro_CheckedChanged;
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(224, 14);
            textBoxPort.Margin = new Padding(4, 3, 4, 3);
            textBoxPort.MaxLength = 6;
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(48, 23);
            textBoxPort.TabIndex = 10;
            textBoxPort.Text = "9000";
            textBoxPort.TextChanged += textBoxPort_TextChanged;
            textBoxPort.KeyPress += textBoxPort_KeyPress;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(183, 17);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(32, 15);
            label3.TabIndex = 11;
            label3.Text = "Port:";
            // 
            // textBoxFilePath
            // 
            textBoxFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxFilePath.Location = new Point(351, 45);
            textBoxFilePath.Margin = new Padding(4, 3, 4, 3);
            textBoxFilePath.Name = "textBoxFilePath";
            textBoxFilePath.ReadOnly = true;
            textBoxFilePath.Size = new Size(174, 23);
            textBoxFilePath.TabIndex = 12;
            textBoxFilePath.TextChanged += textBoxFilePath_TextChanged;
            // 
            // buttonSelectFile
            // 
            buttonSelectFile.Location = new Point(667, 48);
            buttonSelectFile.Margin = new Padding(4, 3, 4, 3);
            buttonSelectFile.Name = "buttonSelectFile";
            buttonSelectFile.Size = new Size(88, 25);
            buttonSelectFile.TabIndex = 13;
            buttonSelectFile.Text = "Seleccionar";
            buttonSelectFile.UseVisualStyleBackColor = true;
            buttonSelectFile.Click += buttonSelectFile_Click;
            // 
            // buttonOpenFile
            // 
            buttonOpenFile.Location = new Point(533, 48);
            buttonOpenFile.Margin = new Padding(4, 3, 4, 3);
            buttonOpenFile.Name = "buttonOpenFile";
            buttonOpenFile.Size = new Size(112, 25);
            buttonOpenFile.TabIndex = 14;
            buttonOpenFile.Text = "Abrir en Reprod.";
            buttonOpenFile.UseVisualStyleBackColor = true;
            buttonOpenFile.Click += buttonOpenFile_Click;
            // 
            // webBrowserPlayer
            // 
            webBrowserPlayer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowserPlayer.Location = new Point(15, 86);
            webBrowserPlayer.Margin = new Padding(4, 3, 4, 3);
            webBrowserPlayer.Name = "webBrowserPlayer";
            webBrowserPlayer.Size = new Size(896, 318);
            webBrowserPlayer.TabIndex = 16;
            // 
            // videoView
            // 
            videoView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            videoView.BackColor = Color.Black;
            videoView.Location = new Point(15, 86);
            videoView.Margin = new Padding(4, 3, 4, 3);
            videoView.MediaPlayer = null;
            videoView.Name = "videoView";
            videoView.Size = new Size(896, 318);
            videoView.TabIndex = 15;
            // 
            // timerVlcStatus
            // 
            timerVlcStatus.Interval = 500;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 519);
            Controls.Add(label3);
            Controls.Add(textBoxPort);
            Controls.Add(checkBoxMaestro);
            Controls.Add(buttonSearch);
            Controls.Add(labelDebug);
            Controls.Add(buttonConnect);
            Controls.Add(label2);
            Controls.Add(textHost);
            Controls.Add(label1);
            Controls.Add(pName);
            Controls.Add(pauseButton);
            Controls.Add(videoView);
            Controls.Add(webBrowserPlayer);
            Controls.Add(textBoxFilePath);
            Controls.Add(buttonSelectFile);
            Controls.Add(buttonOpenFile);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Text = "Pinículas Sincronizadas";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)videoView).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.TextBox pName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textHost;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelDebug;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.CheckBox checkBoxMaestro;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Button buttonSelectFile;
        private System.Windows.Forms.Button buttonOpenFile;
        private System.Windows.Forms.WebBrowser webBrowserPlayer;
        private LibVLCSharp.WinForms.VideoView videoView;
        // comboVout and buttonReinitVlc removed — automatic video output only
        private System.Windows.Forms.Timer timerVlcStatus;
        // checkDisableHw removed
    }
}

