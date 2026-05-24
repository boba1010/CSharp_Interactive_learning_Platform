using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views;

[QueryProperty(nameof(BattleId), "BattleId")]
public partial class BattlePage : ContentPage
{
	public int BattleId { get; set; }

	BattleViewModel ViewModel { get; set; }
	public BattlePage(BattleViewModel vm)
	{
		InitializeComponent();
		ViewModel = vm;
		BindingContext = ViewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.RequestBattleStartAsync(BattleId);

        Random ran = new();
        for (int i = 0; i < ViewModel.EnemiesNumber; i++)
            fightingArena.Add(new Label() { Text = "👾", FontSize = 40, Margin = new(ran.Next(500), ran.Next(150), 0, 0), HorizontalOptions = LayoutOptions.Center });
    }
}