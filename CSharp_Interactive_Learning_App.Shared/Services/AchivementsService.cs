using CSharp_Interactive_Learning_App.Shared.Models;
using System.Net.Http.Json;

namespace CSharp_Interactive_Learning_App.Shared.Services;

public sealed class AchivementsService(ApiClient apiClient) : IAchivementsService
{
    private const string Uri = "https://localhost:7279/api/achivements";
    //private const string Uri = "https://csharp-interactive-learning-platform.onrender.com/api/achivements";

    public async Task<ServiceResult<List<Achievement>>> GetAchivementsAsync()
    {
        try
        {
            var response = await apiClient.SendAsync(client => client.GetAsync(Uri));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return ServiceResult<List<Achievement>>.Failure(statusCode: response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return ServiceResult<List<Achievement>>.Failure(statusCode: response.StatusCode);
            }
            else if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                return ServiceResult<List<Achievement>>.Failure(msg);
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = await response.Content.ReadFromJsonAsync<List<Achievement>>();

            return ServiceResult<List<Achievement>>.Success(result);
        }
        catch (TaskCanceledException)
        {
            return ServiceResult<List<Achievement>>.Failure("Error: Connection Timeout.");
        }
        catch (HttpRequestException)
        {
            return ServiceResult<List<Achievement>>.Failure("Error: Network connection failed.");
        }
        catch (Exception)
        {
            return ServiceResult<List<Achievement>>.Failure("Error: An unexpected system error occurred.");
        }
    }

    public async Task<ServiceResult> UnlockAchievementAsync(int id)
    {
        try
        {
            var response = await apiClient.SendAsync(client => client.PostAsJsonAsync($"{Uri}/unlock", id));
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ServiceResult.Failure(statusCode: response.StatusCode);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return ServiceResult.Failure(statusCode: response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                return ServiceResult.Failure(errorMsg: msg);
            }

            return ServiceResult.Success();
        }
        catch (TaskCanceledException)
        {
            return ServiceResult.Failure("Error: Connection Timeout.");
        }
        catch (HttpRequestException)
        {
            return ServiceResult.Failure("Error: Network connection failed.");
        }
        catch (Exception)
        {
            return ServiceResult.Failure("Error: An unexpected system error occurred.");
        }
    }
}
