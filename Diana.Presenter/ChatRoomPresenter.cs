using Diana.Core.Dtos;
using Diana.Core.Interfaces;


namespace Diana.Presenter;

public class ChatRoomPresenter
{

    private readonly IChatRoomView _view;
    private readonly IChatService _chatService;

    public ChatRoomPresenter(IChatRoomView view, IChatService chatService)
    {
        _view = view;
        _chatService = chatService;
        _chatService.OnMessageReceived += HandleRecievedMessage;
        _view.OnSendPressed += async () =>  await OnSendPressed();
        _view.OnLeavePressed += async () => await OnLeavePressed() ;
    }

    private async Task OnLeavePressed()
    {
        await _chatService.SendAsync(ClientRequest.LeaveRoom(_view.RoomKey, _view.Username));
        _chatService.OnMessageReceived -= HandleRecievedMessage;
        _view.CloseView();
    }

    public async Task OnSendPressed()
    {
        
        if (string.IsNullOrWhiteSpace(_view.MessageContent)) return;

        await _chatService.SendAsync(ClientRequest.SendMessage(_view.RoomKey, _view.Username, _view.MessageContent));
        _view.ClearInput();

    }

    public void HandleRecievedMessage( ServerResponse message )
    {
        switch( message.Type )
        {
            case MessageType.SendMessage:
                HandleSendMessage(message); break;

            case MessageType.UserJoined:
                HandleUserJoinedMessage(message); break;

            case MessageType.UserLeft:
                HandleUserLeftMessage(message); break;
        }
    }

    private void HandleUserLeftMessage(ServerResponse message)
    {
        _view.AppendMessage(message.Username, "left the room" , message.ImageBase64);
    }

    private void HandleUserJoinedMessage(ServerResponse message)
    {
        _view.AppendMessage(message.Username, "joined the room", message.ImageBase64);
    }


    private void HandleSendMessage(ServerResponse message)
    {
        _view.AppendMessage(message.Username, message.Content, message.ImageBase64);
    }
}
