using CSharp_Interactive_Learning_App.DTOs.BattleDTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp_Interactive_Learning_App.Services
{
    public class BattleService(HttpClient httpClient, ApiClient apiClient)
    {
        string url = "http://192.168.1.50:5000/api/chapters";

        public async Task<ChaptersRequest?> GetAllChaptersAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("Token");
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);
                var result = await apiClient.SendAsync(() => httpClient.GetAsync($"{url}"));

                ChaptersRequest? response = await result.Content.ReadFromJsonAsync<ChaptersRequest>();
                return response;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", $"{ex}", "OK");
                return null;
            }
        }

        public async Task<BattleResult> RequestLessonAndBattleCompletionAsync(RequestBattleCompletion requestLessonCompletion)
        {
            try
            {
                var token = await SecureStorage.GetAsync("Token");
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

                var jsonString = JsonSerializer.Serialize(requestLessonCompletion);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await apiClient.SendAsync(() => httpClient.PostAsync($"{url}/validateBattle", content));
                if (!result.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlertAsync("Warning", await result.Content.ReadAsStringAsync(), "OK");
                    return new() { IsSuccess = false };
                }

                return await result.Content.ReadFromJsonAsync<BattleResult>() ?? new();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", $"{ex}", "OK");
                return new() { IsSuccess = false };
            }
        }

        public async Task<BattleStartResult> RequestBattleStartAsync(RequestBattleStart requestBattleStart)
        {
            try
            {
                var token = await SecureStorage.GetAsync("Token");
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

                var jsonString = JsonSerializer.Serialize(requestBattleStart);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await apiClient.SendAsync(() =>  httpClient.PostAsync($"{url}/startBattle", content));
                if (!result.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlertAsync("Warning", await result.Content.ReadAsStringAsync(), "OK");
                    return new() { IsSuccess = false };
                }

                return await result.Content.ReadFromJsonAsync<BattleStartResult>() ?? new();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", $"{ex}", "OK");
                return new() { IsSuccess = false };
            }
        }

        public async Task<BattleEndResult> RequestBattleEndAsync(RequestBattleEnd request)
        {
            try
            {
                var token = await SecureStorage.GetAsync("Token");
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

                string jsonstring = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonstring, Encoding.UTF8, "application/json");
                var result = await apiClient.SendAsync(() => httpClient.PostAsync($"{url}/endBattle", content));
                if (!result.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlertAsync("Warning", await result.Content.ReadAsStringAsync(), "OK");
                    return new() { IsSuccess = false };
                }

                return await result.Content.ReadFromJsonAsync<BattleEndResult>() ?? new();
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", "Http request failed. Check your internet then try again.", "OK");
                return new();
            }
            catch (HttpProtocolException)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", "Something went wrong, please try again.", "OK");
                return new();
            }
            catch (HttpIOException)
            {
                await Shell.Current.DisplayAlertAsync("ERROR!", "Something went wrong, please try again.", "OK");
                return new();
            }
        }
    }
}
