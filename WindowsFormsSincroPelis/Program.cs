using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SincroPelis
{
    static class Program
    {
        public static MainForm myForm;
        public static Server server = new Server();
        public static Client client = new Client();
        public static Task serverTask;



        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Socket m_socListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Thread tmpThread = new Thread(new ThreadStart(server.Connect));
            myForm = new MainForm();
            Application.Run(myForm);
        }
    }
}
