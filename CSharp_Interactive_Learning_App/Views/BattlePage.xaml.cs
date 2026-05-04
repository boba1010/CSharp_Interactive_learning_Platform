using CSharp_Interactive_Learning_App.Models;
using CSharp_Interactive_Learning_App.ViewModels;
using System.Text.Json;

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

	string token = "";

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        token = await SecureStorage.GetAsync("Token");

        await ViewModel.RequestBattleStartAsync(token, BattleId);

        Random ran = new();
        for (int i = 0; i < ViewModel.EnemiesNumber; i++)
            fightingArena.Add(new Label() { Text = "👾", FontSize = 40, Margin = new(ran.Next(500), ran.Next(150), 0, 0), HorizontalOptions = LayoutOptions.Center });
    }

	private async void OnRunCodeClicked(object? sender, EventArgs e)
	{
		await ViewModel.RequestLessonCompletion(BattleId, token, CodeArea.Text);
	}

	private async void OnReturnButtonClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}

	private async void OnForceExitButtonClicked(object? sender, EventArgs e)
	{
		await ViewModel.RequestBattleEndAsync(token);
		await Shell.Current.GoToAsync("..");
	}

	private async void OnNextButtonClicked(object? sender, EventArgs e)
	{
        await Shell.Current.GoToAsync($"Battle?BattleId={BattleId + 1}");
    }

	private async void OnRetryButtonClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync($"Battle?BattleId={BattleId}");
	}
}