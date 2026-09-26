using CSharp_Interactive_Learning_App.Shared.Models;
using System.Net.Http.Json;

namespace CSharp_Interactive_Learning_App.Shared.Services;

public sealed class LeaderboardService(ApiClient apiClient) : ILeaderboardService
{
    private const string Uri = "https://localhost:7279/api/leaderboard";
    //private const string Uri = "https://csharp-interactive-learning-platform.onrender.com/api/leaderboard";

    public async Task<ServiceResult<List<UserRank>>> GetLeaderboardAsync()
    {
        try
        {
            var response = await apiClient.SendAsync(client => client.GetAsync(Uri));
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                return ServiceResult<List<UserRank>>.Failure(msg);
            }

            List<UserRank> result = await response.Content.ReadFromJsonAsync<List<UserRank>>() ?? throw new Exception();

            return ServiceResult<List<UserRank>>.Success(result);
        }
        catch (TaskCanceledException)
        {
            return ServiceResult<List<UserRank>>.Failure("Error: Connection Timeout.");
        }
        catch (HttpRequestException)
        {
            return ServiceResult<List<UserRank>>.Failure("Error: Network connection failed.");
        }
        catch (Exception)
        {
            return ServiceResult<List<UserRank>>.Failure("Error: An unexpected system error occurred.");
        }
    }
}
