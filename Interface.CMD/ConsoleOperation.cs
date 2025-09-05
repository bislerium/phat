using phat.Services;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace phat.Interface.CMD
{
    internal class ConsoleOperation
    {
        private readonly MessageService _messageService;
        private const int PollRateMil = 1000 * 2;

        public ConsoleOperation(MessageService messageService)
        {
            _messageService = messageService;
        }

        private void StartMessageReadOperation()
        {
            Thread readerThread = new(async () =>
            {
                await _messageService.ReadMessage((senderEndpoint, messageSize, message) =>
                {
                    ClearCursorLine();
                    AnsiConsole.MarkupLine("   [black on aqua] {0} [/][white on royalblue1] {1}B [/] {2}", getCurrentTime(), messageSize, message);
                    if (Settings.beepOnIncomingMessage) Console.Beep();
                });
            })
            {
                IsBackground = true,
            };
            readerThread.Start();
        }

        private void StartMessageWriteOperation()
        {
            Thread writerThread = new(async () =>
            {
                while (true)
                {
                    string? s = Console.ReadLine();
                    ClearCursorLine(1);
                    if (s is not null && s.Length > 0) await _messageService.WriteMessage(s, (senderEndpoint, messageSize, message) =>
                    {
                        AnsiConsole.MarkupLine("   [black on yellow] {0} [/][white on royalblue1] {1}B [/] {2}", getCurrentTime(), messageSize, message);
                    });
                }
            })
            {
                IsBackground = true,
            };
            writerThread.Start();
        }

        public static void ClearCursorLine(int index = 0)
        {
            Console.SetCursorPosition(0, Console.CursorTop - index);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        internal void Start()
        {
            StartMessageReadOperation();
            StartMessageWriteOperation();
            Socket socketClient = _messageService.Client.Client;
            var f = ConnectionService.GetRemoteClientEndpoint;
            Thread connectionStateThread = new(() =>
            {
                var tp = IPGlobalProperties.GetIPGlobalProperties();

                while (true)
                {
                    Thread.Sleep(PollRateMil);
                    TcpState? tcpState = tp.GetActiveTcpConnections().FirstOrDefault(x => x.LocalEndPoint.Equals(socketClient.LocalEndPoint))?.State;

                    if (tcpState == null || tcpState == TcpState.Unknown || tcpState == TcpState.CloseWait || tcpState == TcpState.Closed)
                    {
                        _messageService.Dispose();
                        break;
                    }
                }
                Rule rule = new("[red]Disconnected[/]")
                {
                    Alignment = Justify.Left
                };
                AnsiConsole.WriteLine("");
                AnsiConsole.Write(rule);
            });
            connectionStateThread.Start();
        }

        public static string getCurrentTime() => DateTime.Now.ToLongTimeString();

    }
}
