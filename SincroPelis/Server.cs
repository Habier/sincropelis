using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SincroPelis
{
    class Server
    {
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private List<Socket> socketList = new List<Socket>();
        private const int _BUFFER_SIZE = 2048;
        public static int PORT = 9000;

        private byte[] _buffer = new byte[_BUFFER_SIZE];
        private bool started = false;

        public void start()
        {
            if (!started)
            {
                Console.WriteLine("Servidor Socket ON");
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                started = true;
            }
        }

        public void startAndConnect()
        {
            Program.server.start();
            Program.client.TryConnect("127.0.0.1");
        }
        
        private void Send2All(string message, Socket? excludeSocket = null)
        {
            foreach (Socket socket in socketList)
            {
                if (socket != excludeSocket)
                {
                    try
                    {
                        byte[] data = Encoding.ASCII.GetBytes(message);
                        socket.Send(data);
                    }
                    catch { }
                }
            }
            Console.WriteLine("Sent to all: " + message);
        }

        private void CloseAll()
        {
            _serverSocket.Close();

            foreach (Socket socket in socketList)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

        }


        /// <summary>
        /// Accepts a new client 
        /// </summary>
        /// <param name="asyncronousResult"></param>
        private void AcceptCallback(IAsyncResult asyncronousResult)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(asyncronousResult);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            socketList.Add(socket);
            socket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            KeyController.Log("New Client: " + socket.RemoteEndPoint?.ToString());
            _serverSocket.BeginAccept(AcceptCallback, null);
        }


        /// <summary>
        /// Server Reciving client data
        /// </summary>
        /// <param name="asyncronousResult"></param>
        private void ReceiveCallback(IAsyncResult asyncronousResult)
        {
            Socket? current = (Socket?)asyncronousResult.AsyncState;
            int received;

            try
            {
                received = current!.EndReceive(asyncronousResult);
            }
            catch (SocketException) // Connection broken/error
            {
                KeyController.Log("Conexión perdida: " + current?.RemoteEndPoint?.ToString());
                current?.Close();
                if (current != null) socketList.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Texto recebido: " + text);


            Send2All(text, current);

            current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }

}