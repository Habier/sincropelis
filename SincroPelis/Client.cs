using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SincroPelis
{
    class Client
    {
        Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int _BUFFER_SIZE = 2048;
        public static int PORT = 9000;

        private byte[] _buffer = new byte[_BUFFER_SIZE];

        public void TryConnect(string ip)
        {
            try
            {
                //if(socketClient.Connected)

                socketClient.Connect(IPAddress.Parse(ip), Server.PORT);
                socketClient.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socketClient);
                KeyController.logThis("Conectado Bien " + Environment.UserName);

            }
            catch
            {
                KeyController.logThis("Error al conectar");
            }
        }

        public void TrySend(string msg = "pause")
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(msg);
                socketClient.Send(data);
            }
            catch (SocketException)
            {
                KeyController.logThis("Error - no se pudo enviar/conectar.");
                return;
            }
        }

        private void ReceiveCallback(IAsyncResult asyncronousResult)
        {
            int received;

            try
            {
                received = socketClient.EndReceive(asyncronousResult);
            }
            catch (SocketException)
            {
                //Console.WriteLine("Conection lost: " + socketClient.RemoteEndPoint.ToString());
                KeyController.logThis("Error - Servidor no accesible");
                socketClient.Close();

                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Texto recebido: " + text);
            KeyController.SendKey2Process(Program.myForm.getProcessName());


            socketClient.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socketClient);
        }
    }
}