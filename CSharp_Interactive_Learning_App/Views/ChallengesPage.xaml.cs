namespace CSharp_Interactive_Learning_App.Views;

public partial class ChallengesPage : ContentPage
{
	public ChallengesPage()
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

    private async void OnPlayGroundButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PlayGround");
    }

    private async void OnLeaderboardButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Leaderboard");
    }
}