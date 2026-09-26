using CSharp_Interactive_Learning_App.Shared.Models;

namespace CSharp_Interactive_Learning_App.Shared.Services;

public interface IUserService
{
    public Task<ServiceResult<User>> GetUserDataAsync();
}
