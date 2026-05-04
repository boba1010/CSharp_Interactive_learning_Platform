using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views.UserViews;

public partial class ProfilePage : ContentPage
{
    UserViewModel ViewModel { get; set; }
	public ProfilePage(UserViewModel vm)
	{
		InitializeComponent();
        ViewModel = vm;
        BindingContext = ViewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadUserInfo();
    }

    private async void OnLeaderboardButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Leaderboard");
    }

    private async void OnHomeButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///SplashScreen/Home", false);
    }

    private async void OnChallengesButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Challenges");
    }

    private async void OnPlayGroundButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PlayGround");
    }
}