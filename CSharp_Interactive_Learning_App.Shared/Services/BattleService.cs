using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using CSharp_Interactive_Learning_App.Shared.Models;
using System.Net.Http.Json;

namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public class BattleService(ApiClient apiClient) : IBattleService
    {
        private const string Uri = "https://localhost:7279/api/chapters";
        //private const string Uri = "https://csharp-interactive-learning-platform.onrender.com/api/user";

        public async Task<ServiceResult<List<Chapter>>> GetAllChaptersAsync()
        {
            try
            {
                var response = await apiClient.SendAsync(client => client.GetAsync(Uri));
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ServiceResult<List<Chapter>>
                        .Failure(statusCode: System.Net.HttpStatusCode.Unauthorized);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ServiceResult<List<Chapter>>
                        .Failure(statusCode: System.Net.HttpStatusCode.Forbidden);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(msg);
                    return ServiceResult<List<Chapter>>.Failure(msg);
                }

                var chapters = await response.Content.ReadFromJsonAsync<Chapters>();
                return ServiceResult<List<Chapter>>.Success(chapters?.ChaptersList);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<List<Chapter>>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException)
            {
                return ServiceResult<List<Chapter>>.Failure("Error: Network connection failed.");
            }
            catch (Exception)
            {
                return ServiceResult<List<Chapter>>.Failure($"Error: An unexpected system error occurred.");
            }
        }

        public async Task<ServiceResult<Battle>> GetBattleByIdAsync(int battleId, int chapterId)
        {
            try
            {
                var response = await apiClient.SendAsync(client => client.GetAsync(Uri + $"/battlebyid?battleId={battleId}&chapterId={chapterId}"));
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return ServiceResult<Battle>.Failure(statusCode: System.Net.HttpStatusCode.Unauthorized);
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return ServiceResult<Battle>.Failure(statusCode: System.Net.HttpStatusCode.Forbidden);
                else if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return ServiceResult<Battle>.Failure(msg);
                }

                var battle = await response.Content.ReadFromJsonAsync<Battle>();
                return ServiceResult<Battle>.Success(battle);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<Battle>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException)
            {
                return ServiceResult<Battle>.Failure("Error: Network connection failed.");
            }
            catch (Exception)
            {
                return ServiceResult<Battle>.Failure($"Error: An unexpected system error occurred.");
            }
        }

        public async Task<ServiceResult<Shared.Contracts.Responses.BattleResult>> ValidateBattleAsync(RequestRoundCompletion requestRoundCompletion)
        {
            try
            {
                var response = await apiClient.SendAsync(client => client.PostAsJsonAsync($"{Uri}/validateBattle", requestRoundCompletion));
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ServiceResult<Shared.Contracts.Responses.BattleResult>
                        .Failure(statusCode: System.Net.HttpStatusCode.Unauthorized);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ServiceResult<Shared.Contracts.Responses.BattleResult>
                        .Failure(statusCode: System.Net.HttpStatusCode.Forbidden);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return ServiceResult<Shared.Contracts.Responses.BattleResult>.Failure(msg);
                }
                var result = await response.Content.ReadFromJsonAsync<Shared.Contracts.Responses.BattleResult>();
                return ServiceResult<Shared.Contracts.Responses.BattleResult>.Success(result);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<Shared.Contracts.Responses.BattleResult>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException)
            {
                return ServiceResult<Shared.Contracts.Responses.BattleResult>.Failure("Error: Network connection failed.");
            }
            catch (Exception)
            {
                return ServiceResult<Shared.Contracts.Responses.BattleResult>.Failure($"Error: An unexpected system error occurred.");
            }
        }

        public async Task<ServiceResult<BattleStartResponse>> StartBattleAsync(RequestBattleStart requestBattleStart)
        {
            try
            {
                var response = await apiClient.SendAsync(client => client.PostAsJsonAsync($"{Uri}/startBattle", requestBattleStart));
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ServiceResult<BattleStartResponse>
                        .Failure(statusCode: System.Net.HttpStatusCode.Unauthorized);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ServiceResult<BattleStartResponse>
                        .Failure(statusCode: System.Net.HttpStatusCode.Forbidden);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return ServiceResult<BattleStartResponse>.Failure(msg);
                }

                var result = await response.Content.ReadFromJsonAsync<BattleStartResponse>();
                return ServiceResult<BattleStartResponse>.Success(result);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<BattleStartResponse>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException)
            {
                return ServiceResult<BattleStartResponse>.Failure("Error: Network connection failed.");
            }
            catch (Exception)
            {
                return ServiceResult<BattleStartResponse>.Failure($"Error: An unexpected system error occurred.");
            }
        }

        public async Task<ServiceResult<bool>> EndBattleAsync(RequestBattleEnd request)
        {
            try
            {
                var response = await apiClient.SendAsync(client => client.PostAsJsonAsync($"{Uri}/endBattle", request));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ServiceResult<bool>
                        .Failure(statusCode: System.Net.HttpStatusCode.Unauthorized);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ServiceResult<bool>
                        .Failure(statusCode: System.Net.HttpStatusCode.Forbidden);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return ServiceResult<bool>.Failure(msg);
                }

                var result = await response.Content.ReadFromJsonAsync<bool>();
                return ServiceResult<bool>.Success(result);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<bool>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException)
            {
                return ServiceResult<bool>.Failure("Error: Network connection failed.");
            }
            catch (Exception)
            {
                return ServiceResult<bool>.Failure($"Error: An unexpected system error occurred.");
            }
        }
    }
}
