using CommandDotNet;
using phat.Services;
using System.Net;
using System.Net.Sockets;

namespace phat.Interface.CMD
{
    internal class ConsoleCommand
    {
        [DefaultCommand()]
        public void Info()
        {
            Console.WriteLine("Welcome to Phat!");
            Console.WriteLine("A console-based P2P Chat Application right to your terminal.");
        }

        [Command("host")]
        public void Host([Option('b')] bool beep, int port = 51123)
        {
            TcpClient remoteClient = ConnectionService.Create(
                new IPEndPoint(ConnectionService.GetLocalIPAddress(), port),
                localEndpoint =>
                {
                    var f = ConnectionService.FlattenIPEndpoint(localEndpoint);
                    Console.WriteLine("Chat session started at {0}:{1}.", f.Item1, f.Item2);
                    Console.WriteLine("Waiting to join...");
                },
                remoteEndpoint =>
                {
                    var f = ConnectionService.FlattenIPEndpoint(remoteEndpoint);
                    Console.WriteLine("{0}:{1} Joined.\n", f.Item1, f.Item2);
                });
            StartMessaging(remoteClient);
            Preference.beepOnIncomingMessage = beep;
        }

        [Command("join")]
        public void Join([Option('b')] bool beep, string ipAddress, int port)
        {
            TcpClient remoteClient = ConnectionService.Join(new IPEndPoint(IPAddress.Parse(ipAddress), port), remoteEndpoint =>
            {
                var f = ConnectionService.FlattenIPEndpoint(remoteEndpoint);
                Console.WriteLine("Connected to {0}:{1}", f.Item1, f.Item2);
            });
            StartMessaging(remoteClient);
            Preference.beepOnIncomingMessage = beep;
        }

        private void StartMessaging(TcpClient client)
        {
            MessageService ms = new (client);
            Console.CancelKeyPress += (args, sender) => {
                ms.Close();
                Console.WriteLine("\nEXITED!\n");
            };
            ConsoleOperation co = new (ms);
            co.Start();
        }
    }
}
