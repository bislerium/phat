using CommandDotNet;
using phat.Services;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

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


        [Command("host", Description ="Host a local chat session", ArgumentSeparatorStrategy = ArgumentSeparatorStrategy.PassThru)]
        public void Host(CommandContext ctx, [Option('b')] bool beep)
        {
            TcpClient onStart(TcpListener listener)
            {
                var f = ConnectionService.FlattenIPEndpoint((IPEndPoint)listener.LocalEndpoint);

                return AnsiConsole.Status()
                .Start($"Chat session started at {f.Item1}:{f.Item2}", ctx =>
                {
                    TcpClient client = listener.AcceptTcpClient();
                    onConnectPrint(client, true);
                    return client;
                });
            }
            var args = ctx.ParseResult!.SeparatedArguments;
            if (args.Count > 2) { }
            string? ip; int? port;
            for (var i in args) {
                bool b = int.TryParse(i, out port);
                if (i is not null) { }
                var r = new Regex(Settings.IPAddressRegex);
                if (r.IsMatch(i)) { 
                
                }
            }
            TcpClient remoteClient = 1 == 1
                ? ConnectionService.Create(onStart)
                : ConnectionService.Create(ConnectionService.GetLocalIPAddress(), 34223, onStart);

            StartMessaging(remoteClient);
            Settings.beepOnIncomingMessage = beep;
        }

        [Command("join", Description = "Join a chat session")]
        public void Join([Option('b')] bool beep, [RegularExpression(Settings.IPAddressRegex)] string ipAddress, [Range(1024, 65535)] int port)
        {
            TcpClient remoteClient = ConnectionService.Connect(IPAddress.Parse(ipAddress), port, client =>
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
            }, 5, 1000);
            StartMessaging(remoteClient);
            Settings.beepOnIncomingMessage = beep;
        }

        private void StartMessaging(TcpClient client)
        {
            MessageService ms = new(client);
            Console.CancelKeyPress += (args, sender) =>
            {
                ms.Dispose();
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
            var l = ConnectionService.FlattenIPEndpoint(ConnectionService.GetLocalClientEndpoint(client)!);

            Rule rule = new($"[green]Connected at {DateTime.Now} :[/] [black on yellow] {l.Item1}:{l.Item2} (You) [/] {(reverse ? "<-" : "->")} [black on aqua] {r.Item1}:{r.Item2} [/]")
            {
                Alignment = Justify.Left
            };
            AnsiConsole.WriteLine("");
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine("");
        }

    }
}
