using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Interactive_Learning_App.WinUI.Views;

public sealed partial class AchievementsPage : Page
{
    public AchievementsViewModel ViewModel { get; }
    public AchievementsPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<AchievementsViewModel>();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.GetAchievementsAsync();
    }

    private async void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var gridView = (GridView)sender;

        if (gridView.SelectedItem is Achievement achievement)
            await ViewModel.UnlockAchievementAsync(achievement.Id);
    }
}
