using Diana.Core.Dtos;

namespace Diana.Server;

class ChatRoom
{
    public ClientHandler Owner { get; set; }

    public readonly string Key;
    public readonly string Name;

    public Dictionary< string , ClientHandler> Members { get; } = new ();

    public ChatRoom(string key , string name , ClientHandler owner ) : this(key , name)
    {
        this.Owner = owner;
    }

    public ChatRoom(string key , string name )
    {
        this.Key = key;
        this.Name = name;
    }

    public async Task BroadcastMessageAsync ( ServerResponse msg , string excludeUsername = null)
    {
        foreach (var member in Members.Values)
        {
            if (excludeUsername is not null && excludeUsername == member.Username) continue;

            await member.SendAsync(msg);
        }
        
    }

    public bool IsEmpty => Members.Count == 0;

    public bool IsOwner(ClientHandler client) => client.Username == Owner.Username; // TODO : make the comparison better because 


}
