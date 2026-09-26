using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels;

public partial class AchievementsViewModel(IAchivementsService achivementsService) : BaseViewModel
{
    public ObservableCollection<Achievement> Achievements { get; } = [];
    [ObservableProperty]
    public partial int TotalAchievements { get; set; }
    [ObservableProperty]
    public partial int TotalUnlockedAchievements { get; set; }

    public async Task GetAchievementsAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        Achievements.Clear();

        var result = await achivementsService.GetAchivementsAsync();

        if (!result.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", result.ErrorMessage!, IsError: true));
            return;
        }

        TotalAchievements = result.Data!.Count;

        foreach (var achievement in result.Data)
        {
            Achievements.Add(achievement);
            achievement.Locked = true;
            if (achievement.Unlocked)
            {
                achievement.Locked = !achievement.Unlocked;
                TotalUnlockedAchievements++;
            }
        }

        IsBusy = false;
    }

    public async Task UnlockAchievementAsync(int id)
    {
        if (IsBusy)
            return;

        IsBusy = true;

        var result = await achivementsService.UnlockAchievementAsync(id);
        if (!result.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", result.ErrorMessage!, IsError: true));
            return;
        }

        IsBusy = false;

        await GetAchievementsAsync();
    }
}
