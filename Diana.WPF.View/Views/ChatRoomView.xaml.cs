using Diana.Core.Interfaces;
using Diana.WPF.View.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Diana.WPF.View.Views;

public partial class ChatRoomView : Window, IChatRoomView
{
    // Parameterless constructor required for XAML loader
    public ChatRoomView()
    {
        InitializeComponent();
        // initialize defaults to avoid null refs when used before parameterized ctor
        Username = string.Empty;
        RoomKey = string.Empty;
        RoomName = string.Empty;
        RoomOwner = string.Empty;
        ImageBase64 = string.Empty;

        SendButton.Click += (s, e) => OnSendPressed?.Invoke();
        LeaveButton.Click += (s, e) => OnLeavePressed?.Invoke();
    }

    public ChatRoomView(string username, string roomKey, string roomName, string ownerUsername, string imageBase64)
    {
        InitializeComponent();
        Username = username ?? string.Empty;
        RoomKey = roomKey ?? string.Empty;
        RoomName = roomName ?? string.Empty;
        RoomOwner = ownerUsername ?? string.Empty;
        ImageBase64 = imageBase64 ?? string.Empty;

        SendButton.Click += (s, e) => OnSendPressed?.Invoke();
        LeaveButton.Click += (s, e) => OnLeavePressed?.Invoke();
    }

    // Backing fields for values not directly bound to controls
    private string _username = string.Empty;
    private string _roomKey = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value ?? string.Empty;
    }

    public string RoomKey
    {
        get => _roomKey;
        set => _roomKey = value ?? string.Empty;
    }

    public string RoomName
    {
        get => ExecuteOnUi(() => RoomTitle.Text);
        set => ExecuteOnUi(() => RoomTitle.Text = value ?? string.Empty);
    }

    public string RoomOwner
    {
        get => ExecuteOnUi(() => RoomOwnerText.Text);
        set => ExecuteOnUi(() => RoomOwnerText.Text = value ?? string.Empty);
    }

    public string MessageContent
    {
        get => ExecuteOnUi(() => MessageBox.Text);
        set => ExecuteOnUi(() => MessageBox.Text = value ?? string.Empty);
    }

    public string ImageBase64 { get; set; } = string.Empty;

    public event Action OnSendPressed;
    public event Action OnLeavePressed;

    public void AppendMessage(string username, string message, string ImageBase64)
    {
        if (Dispatcher.CheckAccess())
        {
            var bubble = new FriendMessageControl(ImageBase64, username, message);
            bubble.Margin = new Thickness(0, 0, 0, 6);

            MessagesList.Children.Add(bubble);

            MessagesScroller.UpdateLayout();
            MessagesScroller.ScrollToBottom();
            return;
        }

        Dispatcher.Invoke(() => AppendMessage(username, message, ImageBase64));
    }

    public void ClearInput() => ExecuteOnUi(() => MessageBox.Text = string.Empty);
    public void CloseView() => ExecuteOnUi(() => Close());

    // Helper to execute code on UI thread and return default when used in getters
    private void ExecuteOnUi(Action action)
    {
        if (Dispatcher.CheckAccess()) action();
        else Dispatcher.Invoke(action);
    }

    private T ExecuteOnUi<T>(Func<T> func)
    {
        if (Dispatcher.CheckAccess()) return func();
        return (T)Dispatcher.Invoke(func)!;
    }
}
