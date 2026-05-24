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
		await ViewModel.LoadChapters();
    }
}