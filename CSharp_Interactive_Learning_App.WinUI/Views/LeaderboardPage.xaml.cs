using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Interactive_Learning_App.WinUI.Views;

public sealed partial class LeaderboardPage : Page
{
    public LeaderboardViewModel ViewModel { get; }

    public LeaderboardPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<LeaderboardViewModel>();
        Loaded += LeaderboardPage_Loaded;
    }

    private async void LeaderboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadLeadeboardAsync();
    }
}
