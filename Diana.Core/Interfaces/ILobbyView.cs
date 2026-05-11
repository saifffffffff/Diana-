using Diana.Core.Dtos;
namespace Diana.Core.Interfaces;

public interface ILobbyView
{
    string Username { get; }
    string RoomKey { get; }
    string ImageBase64 { get; }

    event Action OnJoinRoomPressed;
    event Action OnCreateRoomPressed;
    event Action OnFormLoaded;
    event Action OnClosed;
    
    void ShowRooms(List<RoomInfo> rooms);
    void ShowError(string message);
    void OpenCreateRoomView(string username , IChatService chatService); // returns true is the room is created successfully
    void NavigateToChatRoom(string roomName , string roomKey,  string username , string Owner, IChatService chatService);

}
