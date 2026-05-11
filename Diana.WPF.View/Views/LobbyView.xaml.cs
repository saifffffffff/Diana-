using Diana.Core.Dtos;
using Diana.Core.Interfaces;
using Diana.Presenter;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Diana.Application;
namespace Diana.WPF.View.Views;

public partial class LobbyView : Window, ILobbyView
{
    private LobbyPresenter _presenter;

    public LobbyView()
    {
        InitializeComponent();

        var chatService = new ChatService();
        _presenter = new LobbyPresenter(this, chatService);

        JoinButton.Click += (s, e) => OnJoinRoomPressed?.Invoke();
        CreateButton.Click += (s, e) => OnCreateRoomPressed?.Invoke();

        RoomsList.ItemsSource = null;
        RoomsList.Visibility = Visibility.Collapsed;
        EmptyState.Visibility = Visibility.Visible;
    }

    public string Username => UsernameBox.Text;
    public string RoomKey => RoomKeyBox.Text;

    public string ImageBase64 => "saif";//UserAvatar.ImageSource.ToString();

    public event Action OnJoinRoomPressed;
    public event Action OnCreateRoomPressed;
    public event Action OnFormLoaded;
    public event Action OnClosed;

    public void ShowRooms(List<RoomInfo> rooms)
    {
        Dispatcher.Invoke(() =>
        {
            RoomsList.ItemsSource = rooms;
            bool hasRooms = rooms is { } && rooms.Count > 0;
            RoomsList.Visibility = hasRooms ? Visibility.Visible : Visibility.Collapsed;
            EmptyState.Visibility = hasRooms ? Visibility.Collapsed : Visibility.Visible;

            // Update status to connected
            StatusDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            StatusText.Text = "connected";
        });
    }

    // called from the presenter which is not on the UI thread, so we need to use Dispatcher to show the message box on the UI thread
    public void ShowError(string message)
    {
            
        Dispatcher.Invoke(() => {
            MessageBox.Show(message);
        });
    }

    public void OpenCreateRoomView(string username, IChatService chatService)
    {
        // Ensure UI work runs on the UI thread
        Dispatcher.Invoke(() =>
        {
            var window = new CreateRoomView();
            var presenter = new CreateRoomPresenter(window, chatService);
            window.Owner = this;
            window.ShowDialog();
        });
    }

    public void NavigateToChatRoom(string roomName, string roomKey, string username, string Owner, IChatService chatService)
    {
        // Creating and showing a Window must happen on the UI thread.
        Dispatcher.Invoke(() =>
        {
            var window = new ChatRoomView(username, roomKey, roomName, Owner , ImageBase64);
            var presenter = new ChatRoomPresenter(window, chatService);
            window.Owner = this;
            this.Hide();
            window.Closed += (s, e) => this.Show();
            window.Show();
        });
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        OnFormLoaded?.Invoke();
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        OnClosed?.Invoke();
    }
}
