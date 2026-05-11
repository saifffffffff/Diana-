using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Diana.Core.Dtos;

namespace Diana.Server;

class ClientHandler
{
    public string Username { get; private set; }
    public string ImageBase64 { get; private set; }
    public string CurrentRoomKey { get; private set; }

    private NetworkStream _stream;
    private readonly TcpClient _tcp;
    private StreamWriter _writer;

    public ClientHandler(TcpClient tcp)
    {
        _tcp = tcp;
        _stream = tcp.GetStream();
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
    }


    public async Task Run()
    {
        StreamReader reader = new StreamReader(_stream);
        string line;

        try
        {
            while ((line = await reader.ReadLineAsync()) != null)
            {
                Console.WriteLine("Line recieved");
                if (string.IsNullOrWhiteSpace(line)) continue;

                var msg = JsonSerializer.Deserialize<ClientRequest>(line);

                await MessageHandlerAsync(msg);

            }
        }
        finally
        {
            await CleanUpAsync();
        }

        Console.WriteLine("Client disconnected.");



    }

    public async Task SendAsync( ServerResponse message)
    {
        Console.WriteLine("Sending back");
        if ( message is null ) return;

        var json = JsonSerializer.Serialize(message);
        
        await _writer.WriteLineAsync(json);
    
    }

    async Task CleanUpAsync()
    {
        // stream writer will dispose the underlying stream and the underlying tcp client will dispose the stream as well
        if ( CurrentRoomKey is not null && ChatServer.ChatRooms.TryGetValue(CurrentRoomKey , out var room))
        {
            room.Members.Remove(this.Username);
            await room.BroadcastMessageAsync(ServerResponse.UserLeft(CurrentRoomKey, this.Username, $"{Username} has left the room"));

        }
        
        await _writer.DisposeAsync();

    }

    // client requests
    public async Task MessageHandlerAsync(ClientRequest msg)
    {
        Console.WriteLine("Message received: " + msg.Type);

        switch (msg.Type)
        {
            case MessageType.JoinRoom:
                await HandleJoinRoomRequestAsync(msg);
                break;

            case MessageType.LeaveRoom:
                await HandleLeaveRoomRequestAsync(msg);
                break;

            case MessageType.SendMessage:
                await HandleSendRequestAsync(msg);
                break;

            case MessageType.GetRoomList:
                await HandleRoomListRequestAsync();
                break;

            case MessageType.CreateRoom:
                await HandleCreateRoomRequestAsync(msg);
                break;

            default:
                break;
        }
    }

    private async Task HandleCreateRoomRequestAsync(ClientRequest msg)
    {

        if (string.IsNullOrWhiteSpace(msg.RoomKey) || string.IsNullOrWhiteSpace(msg.RoomName))
        {
            await SendAsync(ServerResponse.CreateRoomFailed(msg.RoomKey, msg.RoomName, msg.Username, "Invalid room key/name"));
            return;
        }
        
        if (ChatServer.ChatRooms.ContainsKey(msg.RoomKey))
        {
            await SendAsync(ServerResponse.CreateRoomFailed(msg.RoomKey, msg.RoomName, msg.Username, "Room key already exists"));
            return;
        }

        var room = new ChatRoom(msg.RoomKey, msg.RoomName , this ); 

        
        ChatServer.ChatRooms.Add( room.Key, room);

        Console.WriteLine("room created");
        await SendAsync(ServerResponse.CreateRoomSuccess("Room created successfully"));



    }

    private async Task HandleLeaveRoomRequestAsync(ClientRequest msg)
    {
        if (ChatServer.ChatRooms.TryGetValue(CurrentRoomKey, out ChatRoom room))
        {
            
            // is he the owner of this room -> move the owner to random person 
            if (room.IsOwner(this))
                room.Owner = room.Members.Values.FirstOrDefault( member => member.Username != this.Username );
            

            CurrentRoomKey = null;
            
            room.Members.Remove(Username);
            
            if (room.IsEmpty)
            {
                Console.WriteLine("Room {0} is empty and will be removed.", room.Name); // TODO : send a message to all clients that the room is removed to update the UI
                ChatServer.ChatRooms.Remove(room.Key);
            }
            
            else
                await room.BroadcastMessageAsync(ServerResponse.UserLeft(CurrentRoomKey, Username));

        }

    }

    private async Task HandleSendRequestAsync(ClientRequest msg)
    {
        if (ChatServer.ChatRooms.ContainsKey(CurrentRoomKey))
        {
            Console.WriteLine("Image : "+msg.ImageBase64);
            await ChatServer.ChatRooms[CurrentRoomKey].BroadcastMessageAsync(new ServerResponse { Type = MessageType.SendMessage, Content = msg.Content, RoomKey = msg.RoomKey, Username = msg.Username, ImageBase64 = msg.ImageBase64 });
        }

        else
        {
            var content = string.IsNullOrEmpty(CurrentRoomKey) ? "This user is not joined any room currentry" : $"Room with key {msg.RoomKey} is not found";
            await SendAsync(
                ServerResponse.SendingMessageFailed(CurrentRoomKey, this.Username, content)
            );
        }
    }

    private async Task HandleJoinRoomRequestAsync(ClientRequest msg)
    {
        string roomKey = msg.RoomKey;

        // invalid key
        if (string.IsNullOrEmpty(roomKey) || !ChatServer.ChatRooms.TryGetValue(roomKey , out var room))
        {
            await SendAsync (ServerResponse.JoinFailed("Invalid room key. Please check and try again."));
            return;
        }

        // validate business rules
        if ( room.Members.Values.Any(client => client.Username == msg.Username))
        {
            await SendAsync(
                ServerResponse.JoinFailed($"Username {msg.Username} is already taken in this room !", roomKey)
            );

            return;
        }
            
        // leave existing room first if the user is already in a room
        if ( !string.IsNullOrEmpty(CurrentRoomKey) && ChatServer.ChatRooms.TryGetValue(CurrentRoomKey, out ChatRoom oldRoom))
        {
            oldRoom.Members.Remove(Username);
            await oldRoom.BroadcastMessageAsync(ServerResponse.UserLeft(CurrentRoomKey, Username));
            CurrentRoomKey = string.Empty;
        }


        // this is where i fill the client info
        CurrentRoomKey = roomKey;
        Username = msg.Username;
        ImageBase64 = msg.ImageBase64;
        room.Members[Username] = this;

        await SendAsync(ServerResponse.JoinSuccess(roomKey, Username, room.Name , room.Owner.Username));


        await room.BroadcastMessageAsync(
            ServerResponse.UserJoined(roomKey, this.Username) , excludeUsername: this.Username
        );

    }       
        
    private async Task HandleRoomListRequestAsync( )
    {
        var roomsInfo = ChatServer.ChatRooms.Values.Select(room => new RoomInfo 
        { 
            RoomName = room.Name,
            RoomKey = room.Key,
            MemberCount = room.Members.Count 
        }).ToList();
        await SendAsync(
            new ServerResponse {
                Type = MessageType.GetRoomList,
                Content = JsonSerializer.Serialize(roomsInfo)
            }
        );
    }

    // equality members
    public override bool Equals(object obj)
    {
        ClientHandler other = obj as ClientHandler;
        if (other is null)
            return false;

        return Username.Equals(other.Username) && CurrentRoomKey.Equals(other.CurrentRoomKey);
    
    }

    public static bool operator == (ClientHandler right, ClientHandler left)
    {
        if (ReferenceEquals(right, left))
            return true;

        if ( right is null)
        {
            if (left is null)
                return true;

            return false;
        }

        return right.Equals(left);
    }

    public static bool operator !=(ClientHandler right, ClientHandler left) => !(right == left);

    public override int GetHashCode() => HashCode.Combine(Username, CurrentRoomKey);
    
    

}
