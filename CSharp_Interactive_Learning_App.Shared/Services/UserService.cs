using CSharp_Interactive_Learning_App.Shared.Models;
using System.Net.Http.Json;

namespace CSharp_Interactive_Learning_App.Shared.Services;

public class UserService(ApiClient apiClient) : IUserService
{
    private const string Uri = "https://localhost:7279/api/user";
    //private const string Uri = "https://csharp-interactive-learning-platform.onrender.com/api/user";

    public async Task<ServiceResult<User>> GetUserDataAsync()
    {
        try
        {
            var response = await apiClient.SendAsync(client => client.GetAsync(Uri));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return ServiceResult<User>
                    .Failure(statusCode: System.Net.HttpStatusCode.Unauthorized);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return ServiceResult<User>
                    .Failure(statusCode: System.Net.HttpStatusCode.Forbidden);
            }
            else if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                return ServiceResult<User>.Failure(msg);
            }

            var result = await response.Content.ReadFromJsonAsync<User>();

            return ServiceResult<User>.Success(result);
        }
        catch (TaskCanceledException)
        {
            return ServiceResult<User>.Failure("Error: Connection Timeout.");
        }
        catch (HttpRequestException)
        {
            return ServiceResult<User>.Failure("Error: Network connection failed.");
        }
        catch (Exception)
        {
            return ServiceResult<User>.Failure("Error: An unexpected system error occurred.");
        }
    }
}
