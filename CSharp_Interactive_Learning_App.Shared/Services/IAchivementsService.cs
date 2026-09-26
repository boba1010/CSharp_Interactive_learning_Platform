using CSharp_Interactive_Learning_App.Shared.Models;
namespace CSharp_Interactive_Learning_App.Shared.Services;

public interface IAchivementsService
{
    public Task<ServiceResult<List<Achievement>>> GetAchivementsAsync();
    public Task<ServiceResult> UnlockAchievementAsync(int id);
}
