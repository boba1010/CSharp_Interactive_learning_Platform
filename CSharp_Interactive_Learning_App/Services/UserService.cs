using CSharp_Interactive_Learning_App.DTOs.UserDTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp_Interactive_Learning_App.Services
{
    public class UserService(HttpClient httpClient)
    {
        string url = "http://192.168.1.50:5000/api/user";

        public async Task<UserLoginResponse?> LoginAsync(UserLoginRequest request)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/login", content);

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
                var result = await httpClient.PostAsync($"{url}/signup", content);

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

        public async Task<bool> VerifyUserAsync(string token)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(token);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/verify", content);
;
                if (!result.IsSuccessStatusCode)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
