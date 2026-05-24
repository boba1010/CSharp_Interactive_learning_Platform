using CommunityToolkit.Mvvm.ComponentModel;
using CSharp_Interactive_Learning_App.DTOs.UserDTOs;
using CSharp_Interactive_Learning_App.Models;
using CSharp_Interactive_Learning_App.Services;
using System.Text.Json;

namespace CSharp_Interactive_Learning_App.ViewModels
{
    public partial class UserViewModel(UserService userService) : ObservableObject
    {
        [ObservableProperty]
        public partial User User { get; set; }

        public async Task LoadUserInfo()
        {
            var jsonString = await SecureStorage.GetAsync("UserInfo");
            if (string.IsNullOrEmpty(jsonString))
                return;
            var user = JsonSerializer.Deserialize<User>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            User = user;
        }

        public async Task LoginAsync(string email, string password)
        {
            var request = new UserLoginRequest { Email = email, Password = password };

            var response = await userService.LoginAsync(request);
            if (response == null)
            {
                await Shell.Current.DisplayAlertAsync("Error!", "Something went wrong.", "OK");
                return;
            }

            // server will return null if the user is logged in successfully
            if (response.Feedback == null)
            {
                Preferences.Set("ErrorMsg", null);
                var jsonString = JsonSerializer.Serialize(response.User);
                await SecureStorage.SetAsync("UserInfo", jsonString);
                Preferences.Set("IsLoggedIn", true);
                User = response.User;
                await SecureStorage.SetAsync("Token", response.Token);
                await SecureStorage.SetAsync("RefreshToken", response.RefreshToken);
                await Shell.Current.GoToAsync("Home");
                return;
            }

            Preferences.Set("ErrorMsg", response.Feedback);
        }

        public async Task SignupAsync(string fullName, string username, string email, string password)
        {
            var request = new UserSignupRequest { Email = email, Password =  password, Username = username, FullName = fullName };

            var response = await userService.SignupAsync(request);
            if (response == null)
            {
                await Shell.Current.DisplayAlertAsync("Error!", "Something went wrong.", "OK");
                return;
            }

            if (response.Feedback == null)
            {
                Preferences.Set("ErrorMsg", null);
                var jsonString = JsonSerializer.Serialize(response.User);
                await SecureStorage.SetAsync("UserInfo", jsonString);
                Preferences.Set("IsLoggedIn", true);

                User = response.User;

                await SecureStorage.SetAsync("Token", response.Token);
                await SecureStorage.SetAsync("RefreshToken", response.RefreshToken);
                await Shell.Current.GoToAsync("Home");

                return;
            }

            Preferences.Set("ErrorMsg", response.Feedback);
        }


        public async Task<bool> VerifyUser()
        {
            await LoadUserInfo();
            if (User == null)
            {
                Preferences.Set("IsLoggedIn", false);
                return false;
            }
            bool isVerified = await userService.VerifyUserAsync();
            if (isVerified)
            {
                Preferences.Set("IsLoggedIn", true);
                return true;
            }
            else
            {
                Preferences.Set("IsLoggedIn", false);
                return false;
            }
        }
    }
}
