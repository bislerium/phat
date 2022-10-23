using phat.Services;
using System.Net;
using System.Net.Sockets;

if (args.Length == 0)
{
    Console.WriteLine("Welcome to Phat!");
    Console.WriteLine("A console-based P2P Chat Application right to your terminal.");
    return;
}

TcpClient sr;
switch (args[0])
{
    case "c":
        sr = ConnectionService.Start(out IPAddress ipAddress, out int port);
        break;
    case "j":
        if (args.Length != 3) throw new ArgumentException("Please pass IP address & port to join the chat session");
        sr = ConnectionService.Connect(args[1], int.Parse(args[2]));
        break;
    default:
        throw new ArgumentException("Invalid Flag!\nAvailable flags:\n\t-c: Start a chat session.\n\t-j: Join a chat session.\n");
}

MessageService ms = new MessageService(sr);

Thread readerThred = new(async () =>
{
    await ms.ReadMessage();
})
{
    IsBackground = true,
};
readerThred.Start();

Thread writerThread = new(async () =>
{
    while (true)
    {
        Console.Write("You: ");
        string? s = Console.ReadLine();
        if (s is not null) await ms.WriteMessage(s);
    }
})
{
    IsBackground = true,
};
writerThread.Start();


Thread ConnectionStatusThread = new(() =>
{
    while (sr.Connected) ;
    Console.WriteLine("Client Disconnected!");
});

ConnectionStatusThread.Start();


