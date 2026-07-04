using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Models;
using System.Collections.ObjectModel;

namespace CSharp_Interactive_Learning_App.ViewModels
{
    public partial class BattleViewModel(IBattleService battleService) : ObservableObject
    {
        public ObservableCollection<Chapter> Chapters { get; set; } = [];

        public Battle Battle { get; set; }

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

        enum BattleState
        {
            Idle,
            Running,
            Finished
        }
        private BattleState _state;

        [ObservableProperty]
        public partial Shared.Models.BattleResult Result { get; set; }

        [ObservableProperty]
        public partial int PlayerHealth { get; set; }

        [ObservableProperty]
        public partial string Code { get; set; }

        public async Task LoadChapters()
        {
            Chapters.Clear();
            var response = await battleService.GetAllChaptersAsync();
            if (!response.IsSuccess)
            {
                await Shell.Current.DisplayAlertAsync("Error", response.ErrorMessage, "OK");
                return;
            }

            var chapters = response.Data;
            foreach (var chapter in chapters)
            {
                List<Battle> battles = [];
                foreach (var battle in chapter.Battles)
                {
                    battles.Add(new() 
                    { 
                        Content = battle.Content,
                        EnemiesNumber = battle.EnemiesNumber,
                        HealthPerEnemy = battle.HealthPerEnemy,
                        Id = battle.Id,
                        ChapterId = battle.ChapterId,
                        Instructions = battle.Instructions,
                        IsUnlocked = battle.IsUnlocked,
                        Name = battle.Name,
                    });
                }
                Chapters.Add(new() { Id = chapter.Id, Battles = battles, Name = chapter.Name });
            }
        }

        [RelayCommand]
        public async Task RequestLessonCompletion()
        {
            if (_state != BattleState.Running && Result != Shared.Models.BattleResult.None)
                return;

            var response = await battleService.ValidateBattleAsync(new() 
            { 
                BattleId = Battle.Id,
                ChapterId = Battle.ChapterId,
                Code = Code,
            });
            if (!response.IsSuccess)
            {
                await Shell.Current.GoToAsync("///SplashScreen/Home");
                return;
            }

            var result = response.Data;

            TotalXpGained = result.TotalXpGained;

            EnemiesNumber = result.CalculationResult.RemainingEnemies.Count;

            EnemiesHealth = result.CalculationResult.RemainingEnemies.Sum(e => e.Health);
            int damage = EnemiesHealth - CurrentEnemiesHealth;
            TotalDamageDealt = damage;

            TotalSelfDamage = result.CalculationResult.DamageTaken;

            PlayerHealth -= TotalSelfDamage;

            if (result.IsOver)
            {
                _state = BattleState.Finished;
                Result = PlayerHealth <= 0 ? Shared.Models.BattleResult.Defeat : Shared.Models.BattleResult.Victory; 
            }
            if (result.CalculationResult.Errors.Count != 0)
                await Shell.Current.DisplayAlertAsync("Your errors:", string.Join('\n', result.CalculationResult.Errors), "OK");
        }

        public async Task RequestBattleStartAsync(int battleId)
        {
            Code = "";

            if (_state == BattleState.Running)
                return;

            var response = await battleService.StartBattleAsync(new() { BattleId = battleId, ChapterId = Battle.ChapterId });

            if (!response.IsSuccess)
            {
                await Shell.Current.DisplayAlertAsync("Error", response.ErrorMessage, "OK");
                return;
            }

            var result = response.Data;
            if (Battle == null)
            {
                await Shell.Current.DisplayAlertAsync("Warning", "Battle could not be found.", "OK");
                await Shell.Current.GoToAsync("///SplashScreen/Home");
                return;
            }

            EnemiesNumber = result.EnemiesNumber;
            EnemiesHealth = result.EnemiesHealth;

            PlayerHealth = result.PlayerHealth;

            await Shell.Current.DisplayAlertAsync($"PREPARE FOR! {Battle.Name}", Battle.Content, "Continue");
            await Shell.Current.DisplayAlertAsync($"INSTRUCTIONS", Battle.Instructions, "START THE FIGHT");
            _state = BattleState.Running;
            Result = Shared.Models.BattleResult.None;
        }

        [RelayCommand]
        public async Task RequestBattleEndAsync()
        {
            Code = "";
            if (_state != BattleState.Running)
                return;

            var request = new RequestBattleEnd { BattleSessionId = Battle.Id,};
            var result = battleService.EndBattleAsync(request);
            _state = BattleState.Finished;
            Result = Shared.Models.BattleResult.None;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task LoadBattleAsync(Battle battle)
        {
            Code = "";
            _state = BattleState.Idle;
            Result = Shared.Models.BattleResult.None;
            Battle = battle;
            await Shell.Current.GoToAsync($"Battle?BattleId={battle.Id}");
        }

        [RelayCommand]
        public async Task ReturnAsync()
        {
            Code = "";
            _state = BattleState.Idle;
            Result = Shared.Models.BattleResult.None;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task NextAsync()
        {
            Code = "";
            _state = BattleState.Idle;
            Result = Shared.Models.BattleResult.None;
            await Shell.Current.GoToAsync($"Battle?BattleId={Battle.Id + 1}");
        }

        [RelayCommand]
        public async Task RetryAsync()
        {
            Code = "";
            _state = BattleState.Idle;
            Result = Shared.Models.BattleResult.None;
            await Shell.Current.GoToAsync($"Battle?BattleId={Battle.Id}");
        }

        [RelayCommand]
        public async Task GoToLeaderboard()
        {
            await Shell.Current.GoToAsync("Leaderboard", false);
        }

        [RelayCommand]
        public async Task GoToProfile()
        {
            await Shell.Current.GoToAsync("Profile", false);
        }

        [RelayCommand]
        public async Task GoToChallenges()
        {
            await Shell.Current.GoToAsync("Challenges", false);
        }

        [RelayCommand]
        public async Task GoToPlayGround()
        {
            await Shell.Current.GoToAsync("PlayGround", false);
        }

        [RelayCommand]
        public async Task GoToHome()
        {
            await Shell.Current.GoToAsync("///SplashScreen/Home", false);
        }
    }
}