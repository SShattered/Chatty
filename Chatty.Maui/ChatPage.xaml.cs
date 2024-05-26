using Chatty.Client;
using Chatty.Shared;
using System.Collections.ObjectModel;

namespace Chatty.Maui;

public partial class ChatPage : ContentPage
{
    private CClient _client;
    public ObservableCollection<ChatModel> ChatModels { get { return _chatModels; } }
    private ObservableCollection<ChatModel> _chatModels = new ObservableCollection<ChatModel>();

	public ChatPage()
	{
        this.BindingContext = this;
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _client = Singleton.Instance.Client;
        _client.TextMessageReceived += _client_TextMessageReceived;
        _client.ChatDisconnected += _client_ChatDisconnected;
    }

    private void _client_ChatDisconnected()
    {
        App.Current.Dispatcher.Dispatch(() =>
        {
            DisplayAlert("Chatty", "User disconnected", "Close");
        });
    }

    private void _client_TextMessageReceived(TextMessage textMessage)
    {
        App.Current.Dispatcher.Dispatch(() =>
        {
            _chatModels.Add(new ChatModel()
            {
                Message = textMessage.Message,
                ChatUser = ChatModel.User.Sender
            });
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _client.TextMessageReceived -= _client_TextMessageReceived;
        _client.ChatDisconnected -= _client_ChatDisconnected;
    }

    private void btnSend_Clicked(object sender, EventArgs e)
    {
        _client.SendTextMessage(textBox.Text);
        _chatModels.Add(new ChatModel()
        {
            Message = textBox.Text,
            ChatUser = ChatModel.User.Receiver
        });
        textBox.Text = String.Empty;
    }
}