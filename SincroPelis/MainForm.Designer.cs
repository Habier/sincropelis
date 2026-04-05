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
            // videoView removed
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
            fullscreenButton = new Button();
            label2 = new Label();
            textHost = new TextBox();
            buttonConnect = new Button();
            labelDebug = new Label();
            checkBoxMaestro = new CheckBox();
            textBoxPort = new TextBox();
            label3 = new Label();
            textBoxFilePath = new TextBox();
            buttonSelectFile = new Button();
            timerVlcStatus = new System.Windows.Forms.Timer(components);
            videoView = new LibVLCSharp.WinForms.VideoView();
            webBrowserPlayer = new WebBrowser();
            panelControls = new Panel();
            playPauseButton = new Button();
            backButton = new Button();
            forwardButton = new Button();
            trackBarPosition = new TrackBar();
            labelTime = new Label();
            trackBarVolume = new TrackBar();
            comboAudio = new ComboBox();
            comboSub = new ComboBox();
            labelConnectedUsers = new Label();
            ((System.ComponentModel.ISupportInitialize)videoView).BeginInit();
            panelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarPosition).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolume).BeginInit();
            SuspendLayout();
            // 
            // fullscreenButton
            // 
            fullscreenButton.Location = new Point(11, 238);
            fullscreenButton.Margin = new Padding(4, 3, 4, 3);
            fullscreenButton.Name = "fullscreenButton";
            fullscreenButton.Size = new Size(150, 58);
            fullscreenButton.TabIndex = 27;
            fullscreenButton.TabStop = false;
            fullscreenButton.Text = "Pantalla Completa";
            fullscreenButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 17);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 26;
            label2.Text = "Host:";
            // 
            // textHost
            // 
            textHost.Location = new Point(59, 14);
            textHost.Margin = new Padding(4, 3, 4, 3);
            textHost.Name = "textHost";
            textHost.Size = new Size(116, 23);
            textHost.TabIndex = 3;
            textHost.TabStop = false;
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(441, 8);
            buttonConnect.Margin = new Padding(4, 3, 4, 3);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(112, 33);
            buttonConnect.TabIndex = 25;
            buttonConnect.TabStop = false;
            buttonConnect.Text = "Conectar";
            buttonConnect.UseVisualStyleBackColor = true;
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
            // checkBoxMaestro
            // 
            checkBoxMaestro.AutoSize = true;
            checkBoxMaestro.Location = new Point(280, 16);
            checkBoxMaestro.Margin = new Padding(4, 3, 4, 3);
            checkBoxMaestro.Name = "checkBoxMaestro";
            checkBoxMaestro.Size = new Size(153, 19);
            checkBoxMaestro.TabIndex = 24;
            checkBoxMaestro.TabStop = false;
            checkBoxMaestro.Text = "Conectar como servidor";
            checkBoxMaestro.UseVisualStyleBackColor = true;
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(224, 14);
            textBoxPort.Margin = new Padding(4, 3, 4, 3);
            textBoxPort.MaxLength = 6;
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(48, 23);
            textBoxPort.TabIndex = 10;
            textBoxPort.TabStop = false;
            textBoxPort.Text = "9000";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(183, 17);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(32, 15);
            label3.TabIndex = 23;
            label3.Text = "Port:";
            // 
            // textBoxFilePath
            // 
            textBoxFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxFilePath.Location = new Point(167, 49);
            textBoxFilePath.Margin = new Padding(4, 3, 4, 3);
            textBoxFilePath.Name = "textBoxFilePath";
            textBoxFilePath.ReadOnly = true;
            textBoxFilePath.Size = new Size(514, 23);
            textBoxFilePath.TabIndex = 12;
            textBoxFilePath.TabStop = false;
            // 
            // buttonSelectFile
            // 
            buttonSelectFile.Location = new Point(689, 49);
            buttonSelectFile.Margin = new Padding(4, 3, 4, 3);
            buttonSelectFile.Name = "buttonSelectFile";
            buttonSelectFile.Size = new Size(88, 25);
            buttonSelectFile.TabIndex = 28;
            buttonSelectFile.TabStop = false;
            buttonSelectFile.Text = "Seleccionar";
            buttonSelectFile.UseVisualStyleBackColor = true;
            // 
            // timerVlcStatus
            // 
            timerVlcStatus.Interval = 500;
            // 
            // videoView
            // 
            videoView.BackColor = Color.Black;
            videoView.Location = new Point(167, 79);
            videoView.MediaPlayer = null;
            videoView.Name = "videoView";
            videoView.Size = new Size(610, 283);
            videoView.TabIndex = 21;
            videoView.TabStop = false;
            // 
            // webBrowserPlayer
            // 
            webBrowserPlayer.Location = new Point(167, 79);
            webBrowserPlayer.Name = "webBrowserPlayer";
            webBrowserPlayer.Size = new Size(610, 283);
            webBrowserPlayer.TabIndex = 0;
            webBrowserPlayer.TabStop = false;
            webBrowserPlayer.Visible = false;
            // 
            // panelControls
            // 
            panelControls.BackColor = Color.FromArgb(160, 0, 0, 0);
            panelControls.Controls.Add(playPauseButton);
            panelControls.Controls.Add(backButton);
            panelControls.Controls.Add(forwardButton);
            panelControls.Controls.Add(trackBarPosition);
            panelControls.Controls.Add(labelTime);
            panelControls.Controls.Add(trackBarVolume);
            panelControls.Location = new Point(167, 369);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(610, 58);
            panelControls.TabIndex = 22;
            // 
            // playPauseButton
            // 
            playPauseButton.Location = new Point(5, 15);
            playPauseButton.Name = "playPauseButton";
            playPauseButton.Size = new Size(80, 28);
            playPauseButton.TabIndex = 0;
            playPauseButton.TabStop = false;
            playPauseButton.Text = "Play";
            playPauseButton.UseVisualStyleBackColor = true;
            // 
            // backButton
            // 
            backButton.Location = new Point(90, 15);
            backButton.Name = "backButton";
            backButton.Size = new Size(40, 28);
            backButton.TabIndex = 1;
            backButton.TabStop = false;
            backButton.Text = "<";
            backButton.UseVisualStyleBackColor = true;
            // 
            // forwardButton
            // 
            forwardButton.Location = new Point(135, 15);
            forwardButton.Name = "forwardButton";
            forwardButton.Size = new Size(40, 28);
            forwardButton.TabIndex = 2;
            forwardButton.TabStop = false;
            forwardButton.Text = ">";
            forwardButton.UseVisualStyleBackColor = true;
            // 
            // trackBarPosition
            // 
            trackBarPosition.Location = new Point(185, 10);
            trackBarPosition.Maximum = 1000;
            trackBarPosition.Name = "trackBarPosition";
            trackBarPosition.Size = new Size(300, 45);
            trackBarPosition.TabIndex = 3;
            trackBarPosition.TabStop = false;
            // 
            // labelTime
            // 
            labelTime.ForeColor = Color.White;
            labelTime.Location = new Point(505, 18);
            labelTime.Name = "labelTime";
            labelTime.Size = new Size(80, 20);
            labelTime.TabIndex = 4;
            labelTime.Text = "00:00/00:00";
            // 
            // trackBarVolume
            // 
            trackBarVolume.Location = new Point(590, 3);
            trackBarVolume.Maximum = 100;
            trackBarVolume.Name = "trackBarVolume";
            trackBarVolume.Orientation = Orientation.Vertical;
            trackBarVolume.Size = new Size(45, 52);
            trackBarVolume.TabIndex = 5;
            trackBarVolume.TabStop = false;
            trackBarVolume.Value = 100;
            // 
            // comboAudio
            // 
            comboAudio.DropDownStyle = ComboBoxStyle.DropDownList;
            comboAudio.Location = new Point(11, 132);
            comboAudio.Name = "comboAudio";
            comboAudio.Size = new Size(150, 23);
            comboAudio.TabIndex = 0;
            comboAudio.TabStop = false;
            // 
            // comboSub
            // 
            comboSub.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSub.Location = new Point(11, 157);
            comboSub.Name = "comboSub";
            comboSub.Size = new Size(150, 23);
            comboSub.TabIndex = 1;
            comboSub.TabStop = false;
            // 
            // labelConnectedUsers
            // 
            labelConnectedUsers.AutoSize = true;
            labelConnectedUsers.Location = new Point(843, 504);
            labelConnectedUsers.Name = "labelConnectedUsers";
            labelConnectedUsers.Size = new Size(0, 15);
            labelConnectedUsers.TabIndex = 25;
            labelConnectedUsers.Visible = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 519);
            Controls.Add(comboAudio);
            Controls.Add(comboSub);
            Controls.Add(videoView);
            Controls.Add(panelControls);
            Controls.Add(label3);
            Controls.Add(textBoxPort);
            Controls.Add(checkBoxMaestro);
            Controls.Add(labelDebug);
            Controls.Add(labelConnectedUsers);
            Controls.Add(buttonConnect);
            Controls.Add(label2);
            Controls.Add(textHost);
            Controls.Add(fullscreenButton);
            Controls.Add(textBoxFilePath);
            Controls.Add(buttonSelectFile);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Text = "Pinículas Sincronizadas";
            ((System.ComponentModel.ISupportInitialize)videoView).EndInit();
            panelControls.ResumeLayout(false);
            panelControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarPosition).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolume).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button fullscreenButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textHost;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelDebug;
        private System.Windows.Forms.CheckBox checkBoxMaestro;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Button buttonSelectFile;
        // comboVout and buttonReinitVlc removed — automatic video output only
        private System.Windows.Forms.Timer timerVlcStatus;
        private LibVLCSharp.WinForms.VideoView videoView;
        private System.Windows.Forms.WebBrowser webBrowserPlayer;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.Button playPauseButton;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button forwardButton;
        private System.Windows.Forms.TrackBar trackBarPosition;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.ComboBox comboAudio;
        private System.Windows.Forms.ComboBox comboSub;
        private System.Windows.Forms.Label labelConnectedUsers;
        // checkDisableHw removed
    }
}

