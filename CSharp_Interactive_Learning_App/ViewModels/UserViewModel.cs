using CommunityToolkit.Mvvm.ComponentModel;
using CSharp_Interactive_Learning_App.Shared.DTOs;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using System.Text.Json;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;

namespace CSharp_Interactive_Learning_App.ViewModels
{
    public partial class UserViewModel(IAuthService authService) : ObservableObject
    {
        [ObservableProperty]
        public partial User User { get; set; }

        [ObservableProperty]
        public partial string ErrorMsg { get; set; }

        public bool IsLoggedIn { get; set; }

        public async Task LoadUserInfo()
        {
            var jsonString = await SecureStorage.GetAsync("UserInfo");
            if (string.IsNullOrEmpty(jsonString))
                return;
            var user = JsonSerializer.Deserialize<User>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            User = user;
        }

        private static User MapUser(UserDTO userDTO)
        {
            return new User
            {
                Id = userDTO.Id,
                FullName = userDTO.FullName,
                Email = userDTO.Email,
                Username = userDTO.Username,
                CurrentLevel = userDTO.CurrentLevel,
                TotalXp = userDTO.TotalXp,
                CompletedLessonIds = userDTO.CompletedLessonIds,
                UnlockedLessonIds = userDTO.UnlockedLessonIds,
            };
        }

        public async Task LoginAsync(string email, string password)
        {
            var request = new UserLoginRequest { Email = email, Password = password };

            Preferences.Set("ErrorMsg", null);

            var response = await authService.LoginAsync(request);
            if (!response.IsSuccess)
            {
                ErrorMsg = response.ErrorMessage;
                return;
            }

            var result = response.Data;

            var jsonString = JsonSerializer.Serialize(result.User);
            await SecureStorage.SetAsync("UserInfo", jsonString);

            IsLoggedIn = true;

            User = MapUser(result.User);

            await SecureStorage.SetAsync("Token", result.Token);
            await SecureStorage.SetAsync("RefreshToken", result.RefreshToken);

            await Shell.Current.GoToAsync("Home");
        }

        public async Task SignupAsync(string fullName, string username, string email, string password)
        {
            var request = new UserSignupRequest { Email = email, Password =  password, Username = username, FullName = fullName };

            var response = await authService.SignupAsync(request);
            if (!response.IsSuccess)
            {
                ErrorMsg = response.ErrorMessage;
                return;
            }

            var result = response.Data;

            var jsonString = JsonSerializer.Serialize(result.User);
            await SecureStorage.SetAsync("UserInfo", jsonString);

            IsLoggedIn = true;

            User = MapUser(result.User);

            await SecureStorage.SetAsync("Token", result.Token);
            await SecureStorage.SetAsync("RefreshToken", result.RefreshToken);

            await Shell.Current.GoToAsync("Home");
        }


        public async Task<bool> VerifyUser()
        {
            await LoadUserInfo();
            if (User == null)
            {
                Preferences.Set("IsLoggedIn", false);
                return false;
            }
            var response = await authService.VerifyUserAsync();

            bool isVerified = response.Data;
            if (isVerified)
            {
                IsLoggedIn = true;
                return IsLoggedIn;
            }
            else
            {
                IsLoggedIn = false;
                return IsLoggedIn;
            }
        }
    }
}
