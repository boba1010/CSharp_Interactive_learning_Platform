using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CSharp_Interactive_Learning_App.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
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
