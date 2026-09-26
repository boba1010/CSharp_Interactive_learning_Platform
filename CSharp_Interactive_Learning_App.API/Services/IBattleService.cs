using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;

namespace CSharp_Interactive_Learning_App.API.Services;

public interface IBattleService
{
    public Task EndBattleAsync(RequestBattleEnd request);
    public Task<BattleStartResponse> StartBattleAsync(RequestBattleStart request, User user);
    public Task<BattleResult> ValidateBattleStep(RequestRoundCompletion request, User user);
}
