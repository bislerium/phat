using System.Net.Sockets;
using System.Net;

namespace phat.Services
{

    internal delegate TcpClient onStartHandler(TcpListener listener); 
    internal delegate void onConnectHandler(TcpClient client);
    
    
    internal static class ConnectionService
    {
        internal static TcpClient Create(IPEndPoint localEndpoint, onStartHandler onStart)
        {
            TcpListener listener = new(localEndpoint);
            listener.Start();
            return onStart.Invoke(listener);             
        }

        internal static TcpClient Join(String remoteIP, int remotePort, onConnectHandler onConnect)
        {
            try
            {
                TcpClient client = new(remoteIP, remotePort);
                onConnect.Invoke(client);                
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


