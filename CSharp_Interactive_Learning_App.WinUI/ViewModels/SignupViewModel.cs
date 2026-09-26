using CommunityToolkit.Mvvm.ComponentModel;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using System.Threading.Tasks;
using CSharp_Interactive_Learning_App.WinUI.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.WinUI.Messages;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels
{
    public partial class SignupViewModel(IAuthService authService, ISecureStorage secureStorage) : ObservableObject
    {
        public User User { get; set; } = new();

        [ObservableProperty]
        public partial string ErrorMsg { get; set; }

        [RelayCommand]
        public async Task SignupAsync()
        {
            var request = new UserSignupRequest { Email = User.Email, Password =  User.Password, Username = User.Username, FullName = User.FullName };

            var response = await authService.SignupAsync(request);
            if (!response.IsSuccess)
            {
                ErrorMsg = response.ErrorMessage!;
                User = new();
                return;
            }

            var result = response.Data!;

            await secureStorage.SetAsync("Token", result.Token);
            await secureStorage.SetAsync("RefreshToken", result.RefreshToken);

            WeakReferenceMessenger.Default.Send(new NavbarMessage(false));
            WeakReferenceMessenger.Default.Send(new NavigationMessage(false, typeof(HomePage)));
            User = new();
        }
    }
}
