using Diana.Core.Dtos;
using Diana.Core.Interfaces;


namespace Diana.Presenter;

public class CreateRoomPresenter
{

    private readonly IChatService _chatService;
    private readonly ICreateRoomView _view;

    public CreateRoomPresenter(ICreateRoomView view , IChatService chatService)
    {
        _view = view;
        _chatService  = chatService;

        _view.OnCreatePressed += async () => await OnCreatePressed();
        _chatService.OnMessageReceived += HandleReceivedMessage;

        
    }

    private async Task OnCreatePressed()
    {
        if ( string.IsNullOrWhiteSpace(_view.RoomName) || string.IsNullOrWhiteSpace(_view.RoomKey))
        {
            _view.ShowError("Invalid Room Key / Name");
            return;
        }
        
        _view.RoomKey = _view.RoomKey.Trim();
        _view.RoomName = _view.RoomName.Trim();

        await _chatService.SendAsync(ClientRequest.CreateRoom(_view.RoomKey , _view.RoomName , _view.Username , string.Empty));
    }

    // server -> client
    private void HandleReceivedMessage ( ServerResponse msg )
    {

        switch (msg.Type)
        {
        
            case MessageType.RoomCreateFailed:
                HandleRoomCreateFailedMessage(msg);
                break;
            
            case MessageType.RoomCreateSuccess:
                HandleRoomCreateSuccessMessage(msg);
                break;
      
        }
            
    }

    private void HandleRoomCreateSuccessMessage(ServerResponse msg)
    {
        _chatService.SendAsync(ClientRequest.JoinRoom(_view.RoomKey, _view.Username));
        _view.IsRoomCreated = true;
        _view.CloseView();

    }

    private void HandleRoomCreateFailedMessage(ServerResponse msg)
    {
        _view.ShowError(msg.Content);
    }


}
