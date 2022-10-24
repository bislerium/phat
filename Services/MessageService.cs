using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

namespace phat.Services
{

    internal delegate void onMessageHandler(IPEndPoint senderEndpoint, int messageSize, string message);

    internal class MessageService
    {

        internal TcpClient Client { get; init; }
        private readonly NetworkStream _ns;

        internal MessageService(TcpClient client)
        {
            Client = client;
            _ns = client.GetStream();
        }

        internal async Task ReadMessage(onMessageHandler onMessage)
        {
            
            byte[] msgbuffer = new byte[2];
            int msgSize = 0;
            StringBuilder sb = new();

            IPEndPoint remoteEndpoint = ConnectionService.GetRemoteClientEndpoint(Client)!;

            while (Client.Connected) {
                while (_ns.DataAvailable)
                {
                    msgSize += await _ns.ReadAsync(msgbuffer);
                    String s = Encoding.UTF8.GetString(msgbuffer);
                    sb.Append(s);
                    Array.Clear(msgbuffer);
                }
                if (sb.Length > 0)
                {
                    onMessage.Invoke(remoteEndpoint, msgSize, sb.ToString());
                    sb.Clear();
                    msgSize = 0;
                }
            }
        }

        internal async Task WriteMessage(string message)
        {
            if (Client.Connected) {
                byte[] writeBuffer = Encoding.UTF8.GetBytes(message);
                await _ns.WriteAsync(writeBuffer);
            }
        }

        internal void Close()
        {
            _ns.Close();
            Client.Close();
        }
    }
}
