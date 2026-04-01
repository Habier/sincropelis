using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SincroPelis
{
    public class Client
    {
        Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int _BUFFER_SIZE = 2048;
        public static int PORT = 9000;

        private byte[] _buffer = new byte[_BUFFER_SIZE];

        public event Action<string>? OnMessageReceived;

        public void TryConnect(string ip)
        {
            try
            {
                //if(socketClient.Connected)

                socketClient.Connect(IPAddress.Parse(ip), Server.PORT);
                socketClient.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socketClient);
                Logger.Info($"Client connected to server as {Environment.UserName}");

            }
            catch (Exception ex)
            {
                Logger.Error($"Connection error: {ex.Message}", ex);
            }
        }

        public void TrySend(string msg = "pause")
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(msg);
                socketClient.Send(data);
            }
            catch (SocketException ex)
            {
                Logger.Error($"Failed to send message: {ex.Message}", ex);
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
            catch (SocketException ex)
            {
                Logger.Error($"Connection lost with server: {ex.Message}", ex);
                socketClient.Close();

                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Logger.Debug($"Received message: {text}");

            OnMessageReceived?.Invoke(text);

            socketClient.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socketClient);
        }
    }
}