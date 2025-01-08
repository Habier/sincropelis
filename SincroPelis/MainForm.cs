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

namespace SincroPelis
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
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
    }
}
