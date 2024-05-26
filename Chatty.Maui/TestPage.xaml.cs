using System.Collections.ObjectModel;

namespace Chatty.Maui;

public partial class TestPage : ContentPage
{

    public ObservableCollection<ChatModel> DataClasses { get { return dataClasses; } }
    private readonly ObservableCollection<ChatModel> dataClasses = new ObservableCollection<ChatModel>();

	public TestPage()
	{
        this.BindingContext = this;
        InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var rand = new Random();
        for (int i = 0; i < 100; i++)
            dataClasses.Add(new ChatModel() { Message = "ABCDefghijkl", ChatUser = (ChatModel.User)rand.Next(0,2) });
    }
}