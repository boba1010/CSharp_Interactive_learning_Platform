using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharp_Interactive_Learning_App.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLeaderboard()
    {
        var users = await dbContext.Users.ToListAsync();

        var ranks = users
            .OrderByDescending(u => u.TotalPoints)
            .Select((user, i) => new UserRank
            {
                UserName = user.Username,
                Points = user.TotalPoints,
                Rank = i + 1
            })
            .ToList();
        return Ok(ranks);
    }
}
