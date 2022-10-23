using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

namespace phat.Services
{
    internal class MessageService
    {

        private readonly TcpClient _client;
        private readonly NetworkStream _ns;


        internal MessageService(TcpClient client)
        {
            _client = client;
            _ns = client.GetStream();
        }

        internal async Task ReadMessage()
        {
            
            byte[] msgbuffer = new byte[2];
            int msgSize = 0;
            StringBuilder sb = new();

            IPEndPoint endpoint = (IPEndPoint) _client.Client.RemoteEndPoint!;
            String clientIP = endpoint.Address.ToString();
            int port = endpoint.Port;

            while (_client.Connected) {
                while (_ns.DataAvailable)
                {
                    msgSize += await _ns.ReadAsync(msgbuffer);
                    String s = Encoding.UTF8.GetString(msgbuffer);
                    sb.Append(s);
                }
                if (sb.Length > 0)
                {
                    Console.WriteLine("{0}:{1} ({2}B): {3}", clientIP, port,  msgSize ,sb.ToString());
                    sb.Clear();
                    msgSize = 0;
                    Array.Clear(msgbuffer);
                }
            }
        }

        internal async Task WriteMessage(string message)
        {
            if (_client.Connected) {
                byte[] writeBuffer = Encoding.UTF8.GetBytes(message);
                await _ns.WriteAsync(writeBuffer);
            }
        }

        internal void Close()
        {
            _client.Close();
            _ns.Close();
        }
    }
}
