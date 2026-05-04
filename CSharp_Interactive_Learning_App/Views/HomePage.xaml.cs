using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views;

public partial class HomePage : ContentPage
{
	BattleViewModel ViewModel { get; set; }
	public HomePage(BattleViewModel vm)
	{
		InitializeComponent();
		ViewModel = vm;
		BindingContext = ViewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
		string token = await SecureStorage.GetAsync("Token");
		await ViewModel.LoadChapters(token);
    }

    private async void OnLeaderboardButtonClicked(object? sender, EventArgs e)
	{
        await Shell.Current.GoToAsync("Leaderboard", false);
    }

	private async void OnProfileButtonClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("Profile", false);
	}

	private async void OnChallengesButtonClicked(object? sender, EventArgs e)
	{
        await Shell.Current.GoToAsync("Challenges", false);
    }

	private async void OnPlayGroundButtonClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("PlayGround", false);
	}
}