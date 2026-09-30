using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels;

public partial class BattleViewModel(IBattleService battleService) : BaseViewModel
{
    public Battle Battle { get; set; } = null!;

    [ObservableProperty]
    public partial bool IsNotOver { get; set; } = true;

    [ObservableProperty]
    public partial int EnemiesHealth { get; set; }

    [ObservableProperty]
    public partial int TotalXpGained { get; set; }

    [ObservableProperty]
    public partial int CurrentEnemiesHealth { get; set; }

    [ObservableProperty]
    public partial int EnemiesNumber { get; set; }

    [ObservableProperty]
    public partial int TotalDamageDealt { get; set; }

    [ObservableProperty]
    public partial int TotalSelfDamage { get; set; }
    [ObservableProperty]
    public partial int VariablesCount { get; set; }

    enum BattleState
    {
        Idle,
        Running,
        Finished
    }
    private BattleState _state;

    [ObservableProperty]
    public partial int PlayerHealth { get; set; }

    [ObservableProperty]
    public partial string Code { get; set; }

    public int BattleSessionId { get; set; }

    [RelayCommand]
    public async Task RequestRoundCompletion()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        if (_state != BattleState.Running)
        {
            IsBusy = false;
            return;
        }

        var response = await battleService.ValidateBattleAsync(new() 
        { 
            BattleId = Battle.Id,
            ChapterId = Battle.ChapterId,
            Code = Code,
        });
        if (!response.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", response?.ErrorMessage!, IsError: true));
            return;
        }

        var result = response.Data!;

        if (!string.IsNullOrEmpty(result.Feedback))
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new TeachingTipMessage("Invalid Syntax Used:", result.Feedback));
            return;
        }

        TotalXpGained = result.TotalXpGained;

        if (result.CalculationResult != null)
        {
            EnemiesNumber = result.CalculationResult.RemainingEnemies.Count;

            EnemiesHealth = result.CalculationResult.RemainingEnemies.Sum(e => e.Health);
            TotalDamageDealt += result.CalculationResult.DamageDealt;

            TotalSelfDamage += result.CalculationResult.DamageTaken;

            VariablesCount += result.CalculationResult.Statements.Count;

            PlayerHealth -= TotalSelfDamage;
            if (result.CalculationResult.Errors.Count != 0)
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage($"Your mistakes:", string.Join('\n', result.CalculationResult.Errors), "Continue", null));
        }

        if (result.IsOver)
        {
            _state = BattleState.Finished;
            IsNotOver = false;
            var battleResult = PlayerHealth <= 0 ? Shared.Models.BattleResult.Defeat : Shared.Models.BattleResult.Victory;
            WeakReferenceMessenger.Default.Send(new BattleMessage(battleResult));
        }

        IsBusy = false;
    }

    public async Task LoadBattleAsync(int battleId, int chapterId)
    {
        if (IsBusy)
            return;

        if (_state == BattleState.Running)
            return;

        IsBusy = true;

        var response = await battleService.GetBattleByIdAsync(battleId, chapterId);
        if (!response.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", response?.ErrorMessage!, IsError: true));
            return;
        }

        var result = response.Data;
        Battle = result!;

        IsBusy = false;

        return;
    }

    public async Task StartBattleAsync(int battleId, int chapterId)
    {
        if (IsBusy)
            return;

        if (_state == BattleState.Running)
            return;

        IsBusy = true;

        var response = await battleService.StartBattleAsync(new() { BattleId = battleId, ChapterId = chapterId });

        if (!response.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", response.ErrorMessage!, IsError: true));
            return;
        }

        var result = response.Data!;
        if (Battle == null)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", response.ErrorMessage!, IsError: true));
            return;
        }

        EnemiesNumber = result.EnemiesNumber;
        EnemiesHealth = result.EnemiesHealth;
        BattleSessionId = result.BattleStateId;
        PlayerHealth = result.PlayerHealth;

        var first = new ShowDialogMessage($"PREPARE FOR! {Battle.Name}", Battle.Content, "Continue", null);
        WeakReferenceMessenger.Default.Send(first);

        var second = new ShowDialogMessage($"INSTRUCTIONS {Battle.Name}", Battle.Instructions, "START THE FIGHT", null);
        WeakReferenceMessenger.Default.Send(second);

        WeakReferenceMessenger.Default.Send(new NavbarMessage(true));

        _state = BattleState.Running;
        IsBusy = false;
    }

    [RelayCommand]
    public async Task EndBattleAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        WeakReferenceMessenger.Default.Send(
            new AbandonBattleMessage
            {
                Respond = result => tcs.SetResult(result)
            });


        var confirmed = await tcs.Task;

        if (!confirmed || IsBusy || _state != BattleState.Running)
            return;

        IsBusy = true;

        var request = new RequestBattleEnd { BattleSessionId = BattleSessionId, };
        
        await battleService.EndBattleAsync(request);

        _state = BattleState.Finished;
        IsBusy = true;

        WeakReferenceMessenger.Default.Send(new NavbarMessage(false));
        WeakReferenceMessenger.Default.Send(new NavigationMessage(true));
    }

    [RelayCommand]
    public async Task ReturnAsync()
    {
        _state = BattleState.Idle;
        IsNotOver = true;

        WeakReferenceMessenger.Default.Send(new NavbarMessage(false));
        WeakReferenceMessenger.Default.Send(new NavigationMessage(true));
    }

    [RelayCommand]
    public async Task NextAsync()
    {
        _state = BattleState.Idle;
        IsNotOver = true;
        WeakReferenceMessenger.Default.Send(new NavbarMessage(false));
        WeakReferenceMessenger.Default.Send(new NavigationMessage(true, Parameter:Battle.Id + 1));
    }

    [RelayCommand]
    public async Task RetryAsync()
    {
        _state = BattleState.Idle;
        IsNotOver = true;
        WeakReferenceMessenger.Default.Send(new NavbarMessage(false));
        WeakReferenceMessenger.Default.Send(new NavigationMessage(true, Parameter: Battle.Id));
    }
}