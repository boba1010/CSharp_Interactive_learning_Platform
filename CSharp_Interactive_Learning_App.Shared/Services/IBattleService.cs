using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using CSharp_Interactive_Learning_App.Shared.Models;

namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public interface IBattleService
    {
        public Task<ServiceResult<List<Chapter>>> GetAllChaptersAsync();
        public Task<ServiceResult<Battle>> GetBattleByIdAsync(int battleId, int chapterId);
        public Task<ServiceResult<Shared.Contracts.Responses.BattleResult>> ValidateBattleAsync(RequestRoundCompletion requestRoundCompletion);
        public Task<ServiceResult<BattleStartResponse>> StartBattleAsync(RequestBattleStart requestBattleStart);
        public Task<ServiceResult<bool>> EndBattleAsync(RequestBattleEnd request);
    }
}
