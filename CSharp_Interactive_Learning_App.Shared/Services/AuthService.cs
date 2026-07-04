using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using CSharp_Interactive_Learning_App.Shared.Models;
using System.Net.Http.Json;

namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public class AuthService(HttpClient httpClient, ApiClient apiClient, ITokenService tokenService) : IAuthService
    {
        private const string url = "https://192.168.1.150:5001/api/user";

        public async Task<ServiceResult<UserLoginResponse>> LoginAsync(UserLoginRequest request)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"{url}/auth/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return ServiceResult<UserLoginResponse>.Failure(msg);
                }

                var result = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
                await tokenService.SetTokenAsync(result.Token);
                await tokenService.SetRefreshTokenAsync(result.RefreshToken);
                return ServiceResult<UserLoginResponse>.Success(result);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<UserLoginResponse>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException)
            {
                return ServiceResult<UserLoginResponse>.Failure("Error: Network connection failed.");
            }
            catch (Exception)
            {
                return ServiceResult<UserLoginResponse>.Failure($"Error: An unexpected system error occurred.");
            }
        }

        public async Task<ServiceResult<UserSignupResponse>> SignupAsync(UserSignupRequest request)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"{url}/auth/signup", request);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return ServiceResult<UserSignupResponse>.Failure(msg);
                }

                var result = await response.Content.ReadFromJsonAsync<UserSignupResponse>();
                await tokenService.SetTokenAsync(result.Token);
                await tokenService.SetRefreshTokenAsync(result.RefreshToken);
                return ServiceResult<UserSignupResponse>.Success(result);
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<UserSignupResponse>.Failure("Error: Connection Timeout.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return ServiceResult<UserSignupResponse>.Failure("Error: Network connection failed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ServiceResult<UserSignupResponse>.Failure($"Error: An unexpected system error occurred.");
            }
        }

        public async Task<ServiceResult<bool>> VerifyUserAsync()
        {
            try
            {
                var result = await apiClient.SendAsync(client => client.GetAsync($"{url}/auth/verify"));

                return ServiceResult<bool>.Success(result.IsSuccessStatusCode);
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

        public async Task<ServiceResult<User>> GetUserDataAsync()
        {
            try
            {
                var response = await apiClient.SendAsync(client => client.GetAsync(url));
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
}
