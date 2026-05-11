
namespace Diana.Core.Interfaces;

public interface ICreateRoomView
{

    string RoomName { get; set; }
    string RoomKey { get; set; }
    string Username { get; set; }
    bool IsRoomCreated { get; set; }
    
    event Action OnCreatePressed;
    event Action OnViewClosed;

    void ShowError(string message);
    //void NavigateToChatRoom(string roomName, string roomKey, string username, IChatService chatService);
    void CloseView();
    void HideView();
    void ShowView();
    
}
