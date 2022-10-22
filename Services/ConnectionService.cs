using System.Net.Sockets;
using System.Net;

namespace phat.Services
{
    internal static class ConnectionService
    {
        internal static TcpClient Connect(String remoteHostAddress, int port)
        {
            try
            {
                TcpClient client = new(remoteHostAddress, port);
                    if (client.Connected)
                    {
                        Console.WriteLine("Connected");
                    }
                    return client;
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        internal static TcpClient Start(out IPAddress ipAddress, out int port)
        {
            IPEndPoint ipEndPoint = new(GetLocalIPAddress(), 13000);
            ipAddress= ipEndPoint.Address;
            port= ipEndPoint.Port;
            Console.WriteLine($"Chat session started at {ipAddress}:{port}.");

            TcpListener listener = new(ipEndPoint);
            listener.Start();
            Console.WriteLine("Waiting to join...");
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Someone Joined");
            return client;
        }

        public static IPAddress GetLocalIPAddress()
        {
            return Dns
                .GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}


