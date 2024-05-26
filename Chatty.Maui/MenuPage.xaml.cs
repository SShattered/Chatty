using Chatty.Client;
using Chatty.Shared;
using System.Diagnostics;

namespace Chatty.Maui;

public partial class MenuPage : ContentPage
{
	private CClient client;
	public MenuPage()
	{
		InitializeComponent();

	}

	public void OnFindAChattyClicked(object sender, EventArgs e)
	{
        client.RequestChat((cclient, args) =>
        {

            if (!args.Accepted)
            {
                App.Current.Dispatcher.Dispatch(() =>
                {
                    DisplayAlert("Chatty", $"Sorry, there's only {args.NumOfClients} client(s) available at the moment", "Close");
                });
            }
        });
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        client = Singleton.Instance.Client;
        client.ChatRequestReceived += Client_ChatRequestReceived;
        client.ChatResponseReceived += Client_ChatResponseReceived;
    }

    private void Client_ChatResponseReceived(bool status, Guid refGuid)
    {
        if (status)
        {
            App.Current.Dispatcher.Dispatch(async () =>
            {
                Singleton.Instance.ClientGuid = refGuid;
                await Navigation.PushModalAsync(new ChatPage());
            });
        }
        else
        {
            App.Current.Dispatcher.Dispatch(() =>
            {
                DisplayAlert("Chatty", $"Rejected :(", "Close");
            });
        }
    }

    private void Client_ChatRequestReceived(Action<bool> callback, Guid refGuid)
    {
        App.Current.Dispatcher.Dispatch(async() =>
        {
            var result = await DisplayAlert("Chatty", "Someone requests you for a chat", "Accept", "Cancel");
            callback.Invoke(result);
            if (result)
            {
                Singleton.Instance.ClientGuid = refGuid;
                await Navigation.PushModalAsync(new ChatPage());
            }    
        });
        
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        client.ChatRequestReceived -= Client_ChatRequestReceived;
        client.ChatResponseReceived -= Client_ChatResponseReceived;
    }
}