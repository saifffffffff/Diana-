
namespace Diana.Core.Interfaces;

public interface IChatRoomView
{
    public string Username { get; protected set; }
    public string RoomKey { get; protected set; }
    public string RoomName { get; protected set; }
    public string RoomOwner { get; protected set; }
    public string MessageContent { get; set; }
    public string ImageBase64 { get; set; }
    event Action OnSendPressed;
    event Action OnLeavePressed;
    

    void AppendMessage(string username, string message , string ImageBase64);
    void ClearInput();
    void CloseView();
     




}
