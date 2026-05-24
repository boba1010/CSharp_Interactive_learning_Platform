using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views;

public partial class LeaderboardPage : ContentPage
{
	public LeaderboardPage(HomeViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}