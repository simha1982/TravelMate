namespace TravelMate.Mobile;

public partial class AppShell : Shell
{
    public AppShell(MainPage mainPage)
    {
        InitializeComponent();
        Items.Clear();
        Items.Add(new ShellContent
        {
            Title = "TravelMate",
            Content = mainPage
        });
    }
}
