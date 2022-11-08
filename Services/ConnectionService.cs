using phat.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace phat.Services
{

    internal delegate TcpClient onStartHandler(TcpListener listener);
    internal delegate void onConnectHandler(TcpClient client);


    internal static class ConnectionService
    {
        internal static TcpClient Create(onStartHandler onStart) => Create(GetLocalIPAddress(), 0, onStart);

        internal static TcpClient Create(IPAddress localIP, onStartHandler onStart) => Create(localIP, 0, onStart);

        internal static TcpClient Create(int localPort, onStartHandler onStart) => Create(GetLocalIPAddress(), localPort, onStart);

        internal static TcpClient Create(IPAddress localIP, int localPort, onStartHandler onStart)
        {
            IPEndPoint localEP = new(localIP, localPort);
            TcpListener listener = new(localEP);
            listener.Start();
            return onStart.Invoke(listener);
        }

        private static (TcpClient, IPEndPoint) GetClientWithEndpoint(IPAddress remoteIP, int remotePort) => (new TcpClient(AddressFamily.InterNetwork), new IPEndPoint(remoteIP, remotePort));

        private static void Connect(TcpClient client, IPEndPoint remoteEP, onConnectHandler onConnect)
        {
            client.Connect(remoteEP);
            onConnect.Invoke(client);
        }

        internal static TcpClient Connect(IPAddress remoteIP,
            int remotePort,
            onConnectHandler onConnect,
            [Range(1, 8)] int retry,
            [Range(1_000, 10_000)] int retryDuration)
        {
            int attempt = 0;
            int socketErrorCode = 0;
            (TcpClient client, IPEndPoint remoteEP) = GetClientWithEndpoint(remoteIP, remotePort);
            while (attempt <= retry)
            {
                try
                {
                    Console.WriteLine("Attmepting to connect: {0}", attempt);
                    if (attempt != 0) Thread.Sleep(retryDuration);
                    Connect(client, remoteEP, onConnect);
                    return client;
                }
                catch (SocketException se)
                {
                    attempt++;
                    socketErrorCode = se.ErrorCode;
                }
            }
            client.Dispose();
            throw new ConnectException(socketErrorCode);
        }


        internal static TcpClient Join(IPAddress remoteIP, int remotePort, onConnectHandler onConnect)
        {
            (TcpClient client, IPEndPoint remoteEP) = GetClientWithEndpoint(remoteIP, remotePort);
            try
            {
                Connect(client, remoteEP, onConnect);
                return client;
            }
            catch (SocketException se)
            {
                client.Dispose();
                throw new ConnectException(se.ErrorCode);
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

        public static IPEndPoint? GetLocalClientEndpoint(TcpClient client) => client.Client.LocalEndPoint as IPEndPoint;

        public static (string, int) FlattenIPEndpoint(IPEndPoint ipEndPoint) => (ipEndPoint.Address.ToString(), ipEndPoint.Port);
    }
}


