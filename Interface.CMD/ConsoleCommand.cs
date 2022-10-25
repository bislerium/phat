using CommandDotNet;
using phat.Services;
using Spectre.Console;
using System.Net;
using System.Net.Sockets;

namespace phat.Interface.CMD
{
    internal class ConsoleCommand
    {
        [DefaultCommand()]
        public void Info()
        {
            AnsiConsole.Write(new FigletText(Settings.AppTitle)
                .LeftAligned()
                .Color(Color.Red));
            Console.WriteLine(Settings.AppDescription);
        }

        [Command("host")]
        public void Host([Option('b')] bool beep, int port = Settings.DefaultPort)
        {
            IPEndPoint localEndpoint = new IPEndPoint(ConnectionService.GetLocalIPAddress(), port);
            TcpClient remoteClient = ConnectionService.Create(
                localEndpoint,
                listener => 
                {
                    var f = ConnectionService.FlattenIPEndpoint(localEndpoint);

                    return AnsiConsole.Status()
                    .Start($"Chat session started at {f.Item1}:{f.Item2}", ctx => 
                    {
                       TcpClient client = listener.AcceptTcpClient();
                       onConnectPrint(client);
                       return client;
                    });
                }
                );
                
            StartMessaging(remoteClient);
            Settings.beepOnIncomingMessage = beep;
        }

        [Command("join")]
        public void Join([Option('b')] bool beep, string ipAddress, int port)
        {
            TcpClient remoteClient = ConnectionService.Join(ipAddress, port, client =>
            {
                var f = ConnectionService.FlattenIPEndpoint(ConnectionService.GetRemoteClientEndpoint(client)!);
                AnsiConsole.Status()
                   .Start("Attempting to Join", ctx =>
                   {
                       if (client.Connected)
                       {
                           onConnectPrint(client);
                       }
                   });
            });
            StartMessaging(remoteClient);
            Settings.beepOnIncomingMessage = beep;
        }

        private void StartMessaging(TcpClient client)
        {
            MessageService ms = new (client);
            Console.CancelKeyPress += (args, sender) => {
                ms.Close();
                Rule rule = new("[red]Exited[/]")
                {
                    Alignment = Justify.Left
                };
                AnsiConsole.WriteLine("");
                AnsiConsole.Write(rule);
                AnsiConsole.WriteLine("");
            };
            ConsoleOperation co = new (ms);
            co.Start();
        }

        void onConnectPrint(TcpClient client) {
            var f = ConnectionService.FlattenIPEndpoint(ConnectionService.GetRemoteClientEndpoint(client)!);
            Rule rule = new($"[green]Connected[/] -> [green]{f.Item1}:{f.Item2}[/]")
            {
                Alignment = Justify.Left
            };
            AnsiConsole.WriteLine("");
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine("");
        }
    }
}
