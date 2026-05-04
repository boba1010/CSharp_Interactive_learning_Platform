using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharp_Interactive_Learning_App.DTOs.BattleDTOs;
using CSharp_Interactive_Learning_App.Models;
using CSharp_Interactive_Learning_App.Services;
using System.Collections.ObjectModel;

namespace CSharp_Interactive_Learning_App.ViewModels
{
    public partial class BattleViewModel(BattleService battleService) : ObservableObject
    {
        public ObservableCollection<Chapter> Chapters { get; set; } = [];

        public int BattleId { get; set; }

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
        public partial bool IsBattleOver { get; set; }

        [ObservableProperty]
        public partial int PlayerHealth { get; set; }

        public async Task LoadChapters(string token)
        {
            Chapters.Clear();
            var response = await battleService.GetAllUnitsAsync(token);
            if (response == null)
                return;
            foreach (var chapter in response.Chapters)
            {
                List<Battle> battles = [];
                foreach (var battle in chapter.Battles)
                    battles.Add(new() { Content = battle.Content, EnemiesNumber = battle.EnemiesNumber, HealthPerEnemy = battle.HealthPerEnemy, Id = battle.Id, Instructions = battle.Instructions, IsUnlocked = battle.IsUnlocked, Name = battle.Name, });
                Chapters.Add(new() { Id = chapter.Id, Battles = battles.ToObservableCollection(), Name = chapter.Name });
            }
        }

        public async Task RequestLessonCompletion(int battleId, string token, string code)
        {
            var result = await battleService.RequestLessonAndBattleCompletionAsync(new() { BattleId = battleId, Token = token, Code = code, });
            if (!result.IsSuccess)
            {
                await Shell.Current.GoToAsync("///SplashScreen/Home");
                return;
            }

            TotalXpGained = result.CalculationResult.TotalXpGained;

            EnemiesNumber = result.CalculationResult.RemainingEnemies.Count;

            EnemiesHealth = result.CalculationResult.RemainingEnemies.Sum(e => e.Health);
            int damage = EnemiesHealth - CurrentEnemiesHealth;
            TotalDamageDealt = damage;

            TotalSelfDamage = result.CalculationResult.SelfDamage;

            PlayerHealth -= TotalSelfDamage;

            if (result.IsOver)
                IsBattleOver = true;
            if (result.CalculationResult.Errors.Count != 0)
                await Shell.Current.DisplayAlertAsync("Your errors:", string.Join('\n', result.CalculationResult.Errors), "OK");
        }

        public async Task RequestBattleStartAsync(string token, int battleId)
        {
            IsBattleOver = false;
            var result = await battleService.RequestBattleStartAsync(new() { BattleId = battleId, Token = token });

            Battle battle = null;
            foreach (var unit in Chapters)
            {
                foreach (var battle_ in unit.Battles)
                {
                    if (battleId == battle_.Id)
                    {
                        battle = battle_;
                        break;
                    }
                }
            }

            if (battle == null)
            {
                await Shell.Current.DisplayAlertAsync("Warning", "Lesson cannot be found.", "OK");
                await Shell.Current.GoToAsync("///SplashScreen/Home");
                return;
            }

            EnemiesNumber = result.EnemiesNumber;
            EnemiesHealth = result.EnemiesHealth;

            PlayerHealth = result.PlayerHealth;

            BattleId = result.Id;

            await Shell.Current.DisplayAlertAsync($"PREPARE FOR! {battle.Name}", battle.Content, "Continue");
            await Shell.Current.DisplayAlertAsync($"INSTRUCTIONS", battle.Instructions, "START THE FIGHT");
        }

        public async Task RequestBattleEndAsync(string token)
        {
            var request = new RequestBattleEnd { BattleId = BattleId, Token = token };
            var result = battleService.RequestBattleEndAsync(request);
            IsBattleOver = true;
            await Shell.Current.GoToAsync("///SplashScreen/Home");
        }

        [RelayCommand]
        public async Task LoadBattle(int battleId)
        {
            await Shell.Current.GoToAsync($"Battle?BattleId={battleId}");
        }
    }
}