using Diana.Core.Interfaces;
using Diana.Presenter;
using System.Windows;

namespace Diana.WPF.View.Views;

public partial class CreateRoomView : Window, ICreateRoomView
{
    public CreateRoomView(string username = "")
    {
        InitializeComponent();
        CreateButton.Click += (s, e) => OnCreatePressed?.Invoke();
        this.Username = username ?? string.Empty;
    }

    public string RoomName
    {
        get
        {
            if (RoomNameBox.Dispatcher.CheckAccess())
                return RoomNameBox.Text;
            return (string)RoomNameBox.Dispatcher.Invoke(() => RoomNameBox.Text);
        }
        set => ExecuteOnUi(() => RoomNameBox.Text = value);
    }

    public string RoomKey
    {
        get
        {
            if (RoomKeyBox.Dispatcher.CheckAccess())
                return RoomKeyBox.Text;
            return (string)RoomKeyBox.Dispatcher.Invoke(() => RoomKeyBox.Text);
        }
        set => ExecuteOnUi(() => RoomKeyBox.Text = value);
    }
    public string Username { get; set; }
    public bool IsRoomCreated { get; set; }

    public event Action OnCreatePressed;
    public event Action OnViewClosed;

    public void ShowError(string message) => ExecuteOnUi(() => MessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
    public void CloseView() => ExecuteOnUi(() => this.Close());
    public void HideView() => ExecuteOnUi(() => this.Hide());
    public void ShowView() => ExecuteOnUi(() => this.Show());

    private void Window_Closed(object sender, System.EventArgs e)
    {
        OnViewClosed?.Invoke();
    }

    private void ExecuteOnUi(Action action)
    {
        if (Dispatcher.CheckAccess()) action();
        else Dispatcher.Invoke(action);
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
