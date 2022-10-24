using System.Net.Sockets;
using System.Net;

namespace phat.Services
{

    internal delegate void onStartHandler(IPEndPoint hostEndpoint); 
    internal delegate void onConnectHandler(IPEndPoint remoteHostAddress);
    
    
    internal static class ConnectionService
    {
        internal static TcpClient Create(IPEndPoint localEndpoint, onStartHandler onStart, onConnectHandler onConnect)
        {
            TcpListener listener = new(localEndpoint);
            listener.Start();
            onStart.Invoke(localEndpoint);

            TcpClient client = listener.AcceptTcpClient();
            IPEndPoint remoteEndpoint = GetRemoteClientEndpoint(client)!;
            onConnect.Invoke(remoteEndpoint);

            return client;
        }

        internal static TcpClient Join(IPEndPoint remoteEndpoint, onConnectHandler onConnect)
        {
            var f = FlattenIPEndpoint(remoteEndpoint);
            try
            {
                TcpClient client = new(f.Item1, f.Item2);
                if (client.Connected) onConnect.Invoke(remoteEndpoint);                
                return client;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            return Dns
                .GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static IPEndPoint? GetRemoteClientEndpoint(TcpClient client) => client.Client.RemoteEndPoint as IPEndPoint;
        
        public static (string, int) FlattenIPEndpoint(IPEndPoint ipEndPoint) => (ipEndPoint.Address.ToString(), ipEndPoint.Port);
    }
}


