using CSharp_Interactive_Learning_App.DTOs.BattleDTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp_Interactive_Learning_App.Services
{
    public class BattleService(HttpClient httpClient)
    {
        string url = "http://192.168.1.50:5000/api/units";

        public async Task<ChaptersRequest?> GetAllUnitsAsync(string token)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(token);
                var result = await httpClient.PostAsync($"{url}", new StringContent(jsonString, Encoding.UTF8, "application/json"));

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
                var jsonString = JsonSerializer.Serialize(requestLessonCompletion);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/completeLesson", content);
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
                var jsonString = JsonSerializer.Serialize(requestBattleStart);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/startBattle", content);
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
                string jsonstring = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonstring, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync($"{url}/endBattle", content);
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
