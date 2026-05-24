using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views;

public partial class PlayGroundPage : ContentPage
{
	public PlayGroundPage(HomeViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}