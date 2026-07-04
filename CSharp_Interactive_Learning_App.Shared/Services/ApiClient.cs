using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using System.Net;
using System.Net.Http.Json;

namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public class ApiClient(HttpClient httpClient, ITokenService tokenService)
    {
        string url = "http://192.168.1.150:5000/api/user";

        public async Task<RefreshTokenResponse?> RefreshSessionAsync(RequestRefreshToken request)
        {
            try
            {
                var result = await httpClient.PostAsJsonAsync($"{url}/auth/refresh", request);

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

        public async Task<HttpResponseMessage> SendAsync(Func<HttpClient, Task<HttpResponseMessage>> request)
        {
            var token = await tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

            var response = await request(httpClient);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            var refreshToken = await tokenService.GetRefreshTokenAsync();

            var refreshed = await RefreshSessionAsync(new() { Token = refreshToken });

            if (refreshed == null)
                return response;

            await tokenService.SetTokenAsync(refreshed.AccessToken);
            await tokenService.SetRefreshTokenAsync(refreshed.RefreshToken);

            return await request(httpClient);
        }
    }
}
