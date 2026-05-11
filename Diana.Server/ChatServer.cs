using Diana.Core.Dtos;
using System.Net;
using System.Net.Sockets;

namespace Diana.Server;

internal class ChatServer
{

    private readonly int port = 5000;


    public readonly static Dictionary< string , ChatRoom> ChatRooms = new ();
    


    public async Task RunAsync()
    {

        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();


        while ( true )
        {
            var tcp = await listener.AcceptTcpClientAsync();
            var client = new ClientHandler(tcp);
            Console.WriteLine("Client connected. {0}", client.Username);
            Task.Run(async () => await client.Run());
        }

    }

    public static async Task Main ()
    {
        ChatServer server = new ChatServer();
        await server.RunAsync();
    }

    

}
