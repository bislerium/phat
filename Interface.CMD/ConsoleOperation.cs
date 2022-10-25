using phat.Services;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace phat.Interface.CMD
{
    internal class ConsoleOperation
    {
        private readonly MessageService _messageService;
        private readonly (string ip, int port) _remoteHostAddress;
        private const int POLL_RATE_MIL = 1000*2;
        
        public ConsoleOperation(MessageService messageService)
        {
            _messageService = messageService;
            _remoteHostAddress = ConnectionService.FlattenIPEndpoint(ConnectionService.GetRemoteClientEndpoint(_messageService.Client)!);
        }

        private void StartMessageReadOperation() {
            Thread readerThread = new(async () =>
            {
                await _messageService.ReadMessage((senderEndpoint, messageSize, message) => {
                    AnsiConsole.MarkupLine("[green]{0}:{1} ({2}B):[/] {3}", _remoteHostAddress.ip, _remoteHostAddress.port, messageSize, message);
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
                    if (s is not null) await _messageService.WriteMessage(s);
                }
            })
            {
                IsBackground = true,
            };
            writerThread.Start();
        }

        internal void Start()
        {
            StartMessageReadOperation();
            StartMessageWriteOperation();
            Socket socketClient = _messageService.Client.Client;
            var f = ConnectionService.GetRemoteClientEndpoint;

            Thread ConnectionStateThread = new(() =>
            {
                var tp = IPGlobalProperties.GetIPGlobalProperties();
                while (true) {
                    Thread.Sleep(POLL_RATE_MIL);
                    TcpState? tcpState = tp.GetActiveTcpConnections().FirstOrDefault(x => x.LocalEndPoint.Equals(socketClient.LocalEndPoint))?.State;
                    if (tcpState == null || tcpState == TcpState.Unknown || tcpState == TcpState.CloseWait || tcpState == TcpState.Closed)
                    {
                        _messageService.Close();
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
            ConnectionStateThread.Start();
        }

        public static TcpState GetState(Socket socketClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .FirstOrDefault(x => x.LocalEndPoint.Equals(socketClient.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
