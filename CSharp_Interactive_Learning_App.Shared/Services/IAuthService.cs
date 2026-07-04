using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using CSharp_Interactive_Learning_App.Shared.Models;

namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public interface IAuthService
    {
        public Task<ServiceResult<UserLoginResponse>> LoginAsync(UserLoginRequest request);
        public Task<ServiceResult<UserSignupResponse>> SignupAsync(UserSignupRequest request);
        public Task<ServiceResult<bool>> VerifyUserAsync();
        public Task<ServiceResult<User>> GetUserDataAsync();
    }
}
