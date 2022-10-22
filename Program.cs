using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using phat.Services;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

/*Host.CreateDefaultBuilder().ConfigureServices((context, services) => {
    services.AddHostedService<MessageReaderService>();
}).UseConsoleLifetime()
    .Build().Run();*/

if (args.Length == 0) {
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
Thread th = new Thread(() => {
    ms.ReadMessage();
});
th.Start();
await ms.WriteMessage("hello there");

