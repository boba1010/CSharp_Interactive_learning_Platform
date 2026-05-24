using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views;

public partial class ChallengesPage : ContentPage
{
	public ChallengesPage(HomeViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}