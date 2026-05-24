using CSharp_Interactive_Learning_App.DTOs.UserDTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp_Interactive_Learning_App.Services
{
    public class ApiClient(HttpClient httpClient)
    {
        string url = "http://192.168.1.50:5000/api/user";

        public async Task<RefreshTokenResponse?> RefreshSessionAsync(RefreshTokenRequest request)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/auth/refresh", content);

                if (!result.IsSuccessStatusCode)
                    return null;

                var response = await result.Content.ReadFromJsonAsync<RefreshTokenResponse>();
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> request)
        {
            var response = await request();
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            var refreshToken = await SecureStorage.GetAsync("RefreshToken");

            var refreshed = await RefreshSessionAsync(new() { Token = refreshToken });

            if (refreshed == null)
                return response;

            await SecureStorage.SetAsync("Token", refreshed.AccessToken);
            await SecureStorage.SetAsync("RefreshToken", refreshed.RefreshToken);

            return await request();
        }
    }
}
