using CSharp_Interactive_Learning_App.DTOs.UserDTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp_Interactive_Learning_App.Services
{
    public class UserService(HttpClient httpClient, ApiClient apiClient)
    {
        readonly string url = "http://192.168.1.50:5000/api/user";

        public async Task<UserLoginResponse?> LoginAsync(UserLoginRequest request)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/auth/login", content);

                UserLoginResponse? response = null;
                if (!result.IsSuccessStatusCode)
                    response = new() { Feedback = await result.Content.ReadAsStringAsync() };
                else
                    response = await result.Content.ReadFromJsonAsync<UserLoginResponse>();
                return response;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", $"{ex}", "OK");
                return null;
            }
        }

        public async Task<UserSignupResponse?> SignupAsync(UserSignupRequest request)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/auth/signup", content);

                UserSignupResponse? response = null;
                if (!result.IsSuccessStatusCode)
                    response = new() { Feedback = await result.Content.ReadAsStringAsync() };
                else 
                    response = await result.Content.ReadFromJsonAsync<UserSignupResponse>();
                return response;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", $"{ex}", "OK");
                return null;
            }
        }

        public async Task<bool> VerifyUserAsync()
        {
            try
            {
                string token = await SecureStorage.GetAsync("Token");
                if (string.IsNullOrEmpty(token))
                    return false;
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

                var result = await apiClient.SendAsync(() => httpClient.GetAsync($"{url}/auth/verify"));

                return result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
