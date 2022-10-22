using Microsoft.Extensions.Hosting;
using System.Net.Sockets;

namespace phat.Services
{
    internal class MessageService
    {

        private readonly TcpClient _client;
        private readonly StreamReader _sr;
        private readonly StreamWriter _sw;
        private bool _closed;


        internal MessageService(TcpClient client)
        {
            _client = client;
            NetworkStream ns = _client.GetStream();
            _sr = new(ns);
            _sw = new(ns);
            _closed = false;
        }

        internal void ReadMessage()
        {
            while (!_closed && _client.Connected)
            {
                string? message = _sr.ReadToEnd();
                Console.WriteLine("1");
                if (message != null) Console.WriteLine(message);
            }
        }

        internal async Task WriteMessage(string message)
        {
            await _sw.WriteAsync(message);
            _sw.Flush();
        }

        internal void Close()
        {
            _closed = true;
            _client.Close();
            _sr.Close();
            _sw.Close();
        }
    }

    internal class MessageReaderService : IHostedService
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public bool Closed { get; set; } = true;

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
            
            }
            while (!Closed && Client.Connected)
            {
                string? message = Reader.ReadToEnd();
                Console.WriteLine("1");
                if (message != null) Console.WriteLine(message);
            }
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
