using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CSharp_Interactive_Learning_App.API.Controllers;

[ApiController]
[Route("api/achivements")]
[Authorize]
public class AchivementsController(AppDbContext dbContext) : ControllerBase
{
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException();
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetAchivementsAsync()
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Id == GetUserId());
        if (user == null)
            return Forbid();

        var dtos = GetAchievements(user.AchievementIds);

        return Ok(dtos);
    }
    
    private List<AchievementDTO> GetAchievements(List<int> ids)
    {
        var achivements = dbContext.Achievements.ToList();
        List<AchievementDTO> dtos = new(achivements.Count);

        foreach (var achievement in achivements)
        {
            var unlocked = false;

            if (ids.Contains(achievement.Id))
                unlocked = true;

            dtos.Add(new()
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                RewardPoints = achievement.RewardPoints,
                Unlocked = unlocked,
            });
        }

        return dtos;
    }

    [HttpPost("unlock")]
    public async Task<IActionResult> UnlockAchivement([FromBody]int id)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Id == GetUserId());
        if (user == null)
            return Forbid();

        if (CanUnlock(id, user))
        {
            var achievement = dbContext.Achievements.FirstOrDefault(a => a.Id == id);
            if (achievement == null)
                return NotFound();

            if (user.AchievementIds.Contains(achievement.Id))
                return BadRequest("Achievement already unlocked");

            user.AchievementIds.Add(id);
            user.TotalPoints += achievement.RewardPoints;

            await dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest("Achievement cannot be unlocked. Requirements aren't met.");
    }

    private bool CanUnlock(int id, User user)
    {
        var achievement = dbContext.Achievements.FirstOrDefault(a => a.Id == id);
        if (achievement == null)
            return false;

        if (achievement.Id == 1 || achievement.Id == 2)
            return true;

        if (id == 3 && user.AchievementIds.Contains(2))
            return true;

        var value = CanUnlockLevelBased(user.CurrentLevel, id);

        if (id == 4 && user.CompletedLessonIds.Contains(2))
            value = true;
        if (id == 11 && user.CompletedLessonIds.Contains(3))
            value = true;
        if (id == 12 && user.UnlockedLessonIds.Contains(10))
            value = true;
        if (id == 10 && user.UnlockedLessonIds.Contains(9))
            value = true;

        return value;
    }

    private bool CanUnlockLevelBased(int level, int id)
    {
        if (id == 6 && (level == 1 || level == 2 || level == 3 || level == 4))
            return true;
        if (id == 7 && (level == 2 || level == 3 || level == 4))
            return true;
        if (id == 8 && (level == 3 || level == 4))
            return true;
        if (id == 9 && level == 4)
            return true;

        return false;
    }
}
