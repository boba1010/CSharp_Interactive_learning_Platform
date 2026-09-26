using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace CSharp_Interactive_Learning_App.WinUI.Views;

public sealed partial class BattlePage : Page
{
    public BattleViewModel ViewModel { get; set; }

    private int ChapterId { get; set; }
    private int BattleId { get; set; }

    private readonly SemaphoreSlim _dialogSemaphore = new(1, 1);

    public BattlePage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<BattleViewModel>();

        Loaded += OnLoaded;

        WeakReferenceMessenger.Default.Register<BattleMessage>(this, BattleMessageRecieved);
        WeakReferenceMessenger.Default.Register<AbandonBattleMessage>(this, AbandonBattleMessageRecieved);
    }

    private async void AbandonBattleMessageRecieved(object recipient, AbandonBattleMessage message)
    {
        await _dialogSemaphore.WaitAsync();

        try
        {
            ContentDialog abandonDialog = new()
            {
                Title = "Abandon Battle?",
                Content = "Leaving now will count as an immediate defeat. Are you sure?",
                PrimaryButtonText = "Surrender",
                CloseButtonText = "Stay and Fight",
                XamlRoot = XamlRoot
            };

            ContentDialogResult result = await abandonDialog.ShowAsync();

            message.Respond(result == ContentDialogResult.Primary);
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    private async void BattleMessageRecieved(object recipient, BattleMessage message)
    {
        if (message.Result == Shared.Models.BattleResult.Victory)
            victoryScreen.Visibility = Visibility.Visible;
        else if (message.Result == Shared.Models.BattleResult.Defeat)
                defeatScreen.Visibility = Visibility.Visible;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;

        await ViewModel.LoadBattleAsync(BattleId, ChapterId);
        await ViewModel.StartBattleAsync(BattleId, ChapterId);

        Random rand = new();

        for (int i = 0; i < ViewModel.EnemiesNumber; i++)
        {
            fightingArena.Children.Add(
                new TextBlock()
                {
                    Text = "👾",
                    FontSize = 45,
                    Margin = new(rand.Next(1000), rand.Next(250), rand.Next(100), 0)
                });
        }
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is RequestBattleStart request)
        {
            ChapterId = request.ChapterId;
            BattleId = request.BattleId;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);

        base.OnNavigatedFrom(e);
    }
}
