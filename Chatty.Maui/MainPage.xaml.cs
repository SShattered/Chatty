using Chatty.Client;

namespace Chatty.Maui
{
    public partial class MainPage : ContentPage
    {
        private Singleton singleton;
        CClient client;
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert(
                "Chatty",
                "Connect to server?",
                "Yes",
                "No");
            if (!result) return;
            singleton = Singleton.Instance;
            client = singleton.Client;
            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                client.Connect("192.168.1.209");
            else
                client.Connect("127.0.0.1");
        }

        private void Client_Disconnected()
        {
            Dispatcher.Dispatch(async () =>
            {
                await DisplayAlert("Chatty", "Disconnected", "Close");
                App.Current.Quit();
            });
        }

        private void Client_Connected()
        {
            Dispatcher.Dispatch(() =>
            {
                Navigation.PushModalAsync(new MenuPage());
            });
        }

        private void BtnExit_Clicked(object sender, EventArgs e)
        {
            App.Current.Quit();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopModalAsync();
            return base.OnBackButtonPressed();
        }
    }
}
