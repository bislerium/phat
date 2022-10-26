using CommandDotNet;
using phat.Services;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
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
            AnsiConsole.Write(new Panel($"{Settings.AppDescription} [black on yellow rapidblink] {Settings.AppRepoURL} [/] [white on royalblue1] {Settings.AppVersion} [/]"));
        }

        [Command("host")]
        public void Host([Option('b')] bool beep, [Range(1024, 65535)] int port = Settings.DefaultPort)
        {
            IPEndPoint localEndpoint = new(ConnectionService.GetLocalIPAddress(), port);
            TcpClient remoteClient = ConnectionService.Create(
                localEndpoint,
                listener =>
                {
                    var f = ConnectionService.FlattenIPEndpoint(localEndpoint);

                    return AnsiConsole.Status()
                    .Start($"Chat session started at {f.Item1}:{f.Item2}", ctx =>
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        onConnectPrint(client, true);
                        return client;
                    });
                }
                );

            StartMessaging(remoteClient);
            Settings.beepOnIncomingMessage = beep;
        }

        [Command("join")]
        public void Join([Option('b')] bool beep, [RegularExpression(Settings.IPAddressRegex, ErrorMessage = "Invalid IPv4 Address!")] string ipAddress, [Range(1024, 65535)] int port)
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
            MessageService ms = new(client);
            Console.CancelKeyPress += (args, sender) =>
            {
                ms.Close();
                Rule rule = new("[red]Exited[/]")
                {
                    Alignment = Justify.Left
                };
                AnsiConsole.WriteLine("");
                AnsiConsole.Write(rule);
                AnsiConsole.WriteLine("");
            };
            ConsoleOperation co = new(ms);
            co.Start();
        }

        void onConnectPrint(TcpClient client, bool reverse = false)
        {
            var r = ConnectionService.FlattenIPEndpoint(ConnectionService.GetRemoteClientEndpoint(client)!);
            var f = ConnectionService.FlattenIPEndpoint(ConnectionService.GetLocalClientEndpoint(client)!);
            Rule rule = new($"[green]Connected at {DateTime.Now} :[/] [black on yellow] {f.Item1}:{f.Item2} (You) [/] {(reverse ? "<-" : "->")} [black on lime] {r.Item1}:{r.Item2} [/]")
            {
                Alignment = Justify.Left
            };
            AnsiConsole.WriteLine("");
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine("");
        }

    }
}
