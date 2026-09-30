using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.API.Services;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CSharp_Interactive_Learning_App.API.Controllers;

[Authorize]
[ApiController]
[Route("api/chapters")]
public class BattlePostController(AppDbContext dbContext, IBattleService battleService) : ControllerBase
{
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException();
        return userId;
    }

    private User GetUser()
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Id == GetUserId());
        return user ?? throw new UnauthorizedAccessException();
    }

    [HttpPost("startBattle")]
    public async Task<IActionResult> RequestBattleStart([FromBody] RequestBattleStart request)
    {
        try
        {
            var user = GetUser();

            var battleResponse = await battleService.StartBattleAsync(request, user);

            return Ok(battleResponse);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("validateBattle")]
    public async Task<IActionResult> ValidateBattleState([FromBody] RequestRoundCompletion request)
    {
        try
        {
            var user = GetUser();

            var result = await battleService.ValidateBattleStep(request, user);
        
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("endBattle")]
    public async Task<IActionResult> RequestBattleEnd([FromBody] RequestBattleEnd request)
    {
        try
        {
            GetUser();

            await battleService.EndBattleAsync(request);

            return Ok(true);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
