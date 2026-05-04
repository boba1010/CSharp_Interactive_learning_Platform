namespace CSharp_Interactive_Learning_App.Views;

public partial class PlayGroundPage : ContentPage
{
	public PlayGroundPage()
	{
		InitializeComponent();
	}

    private async void OnHomeButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///SplashScreen/Home", false);
    }

    private async void OnProfileButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Profile");
    }

    private async void OnChallengesButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Challenges");
    }

    private async void OnLeaderboardButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Leaderboard");
    }
}