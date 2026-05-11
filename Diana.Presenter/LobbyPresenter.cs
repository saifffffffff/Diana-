using Diana.Core.Dtos;
using Diana.Core.Interfaces;
using System.Text.Json;

namespace Diana.Presenter;

public class LobbyPresenter
{
    private readonly ILobbyView _view;
    private readonly IChatService _chatService;


    // If I want to pass data from presenter to view use the view interface methods or properties
    // or you can think about methods as the commands the presenter gives to the view to perform a specific action
    // if i want to pass data from view to presenter use the view events and properties



    public LobbyPresenter(ILobbyView view , IChatService chatService)
    {  
        
        _view = view;
        _chatService = chatService;
        _chatService.Connect("127.0.0.1", 5000);
        _chatService.OnMessageReceived +=  OnMesssageReceived;
        
        _view.OnJoinRoomPressed += async() => await OnJoinPressed();
        _view.OnFormLoaded += async () => await OnLoaded();
        _view.OnCreateRoomPressed += async () => await OnCreateRoomPressed() ;
        _view.OnClosed += OnClosed;
        
    }

    private async Task OnCreateRoomPressed()
    {

        _view.OpenCreateRoomView(_view.Username, _chatService);
        
        await LoadRoomList();
        
    }

    
    
    private async Task LoadRoomList()
    {
        await _chatService.SendAsync(new ClientRequest { Type = MessageType.GetRoomList });    
    }

    private async Task OnJoinPressed ()
    {

        string username = _view.Username?.Trim() ;
        string roomKey = _view.RoomKey?.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            _view.ShowError("Please enter a username.");
            return;
        }

        if (string.IsNullOrWhiteSpace(roomKey))
        {
            _view.ShowError("Please enter a room key.");
            return;
        }

        await _chatService.SendAsync( ClientRequest.JoinRoom(roomKey , username ));


    }

    private void OnClosed()
    {
        _chatService.Disconnect();
    }

    private async Task OnLoaded()
    {
        await LoadRoomList();
    }

    private void OnMesssageReceived( ServerResponse msg)
    {
        HandleRecievedMessage(msg);
    }

    // server -> client
    private void HandleRecievedMessage (ServerResponse msg )
    {
        switch (msg.Type)
        {
            case MessageType.GetRoomList:
                HandleRoomListMessage(msg);
                break;

            case MessageType.JoinSuccess:
                HandleJoinRoomSuccessMessage(msg);
                break;

            case MessageType.JoinFailed:
                HandleJoinRoomFailedMessage(msg);
                break;
            default:
                break;
        }
        
    }

    private void HandleRoomListMessage(ServerResponse msg)
    {
    
        var rooms = JsonSerializer.Deserialize<List<RoomInfo>>(msg.Content) ?? new ();
        
        
        if (rooms.Count == 0) return;

       
        _view.ShowRooms(rooms); //sends the list of rooms from presenter to view to display
    
    }

    private void HandleJoinRoomFailedMessage ( ServerResponse msg)
    {
        _view.ShowError(msg.Content);
    }

    private void HandleJoinRoomSuccessMessage(ServerResponse msg)
    {
        _view.NavigateToChatRoom(msg.RoomName, msg.RoomKey , msg.Username , msg.Username , _chatService );
    }





}
