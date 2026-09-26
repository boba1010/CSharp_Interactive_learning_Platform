using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CSharp_Interactive_Learning_App.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chapters")]
public sealed class BattleGetController(AppDbContext dbContext) : ControllerBase
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

    private Chapter GetChapterById(int id, bool track = false)
    {
        Chapter chapter = null!;

        if (track)
        {
            chapter = dbContext.Chapters
                .Include(c => c.Battles)
                    .ThenInclude(b => b.RequiredStatements)
                .FirstOrDefault(c => c.Id == id)
            ?? throw new InvalidOperationException("Chapter could not be found");

            return chapter;
        }

        chapter = dbContext.Chapters
            .AsNoTracking()
            .Include(c => c.Battles)
                .ThenInclude(b => b.RequiredStatements)
            .FirstOrDefault(c => c.Id == id)
            ?? throw new InvalidOperationException("Chapter could not be found");

        return chapter;
    }

    private List<Chapter> GetChapters(bool track = false)
    {
        List<Chapter> chapters = [];

        if (track)
        {
            chapters = dbContext.Chapters
            .Include(c => c.Battles)
                .ThenInclude(b => b.RequiredStatements)
            .ToList();

            return chapters;
        }

        chapters = dbContext.Chapters
            .AsNoTracking()
            .Include(c => c.Battles)
                .ThenInclude(b => b.RequiredStatements)
            .ToList();
        return chapters;
    }

    private static List<ChapterDTO> MapChaptersByUserProgress(List<Chapter> chapters, User user)
    {
        List<ChapterDTO> chaptersRequests = [];
        foreach (var chapter in chapters)
        {
            List<BattleDTO> preBattles = [];
            foreach (var battle in chapter.Battles)
            {
                BattleDTO preBattleRequest = new()
                {
                    Content = battle.Content,
                    Id = battle.Id,
                    ChapterId = battle.ChapterId,
                    Name = battle.Name,
                    EnemiesNumber = battle.EnemiesNumber,
                    HealthPerEnemy = battle.HealthPerEnemy,
                    Instructions = battle.Instructions
                };
                if (!user.UnlockedLessonIds.Contains(battle.Id))
                    preBattleRequest.IsUnlocked = false;
                else
                    preBattleRequest.IsUnlocked = true;
                preBattles.Add(preBattleRequest);
            }
            chaptersRequests.Add(new() { Id = chapter.Id, Battles = preBattles, Name = chapter.Name });
        }
        return chaptersRequests;
    }

    private static BattleDTO MapBattleByUserProgress(Battle battle, User user)
    {
        if (user.UnlockedLessonIds.Contains(battle.Id))
        {
            BattleDTO BattleDTO = new()
            {
                Content = battle.Content,
                Id = battle.Id,
                ChapterId = battle.ChapterId,
                Name = battle.Name,
                EnemiesNumber = battle.EnemiesNumber,
                HealthPerEnemy = battle.HealthPerEnemy,
                Instructions = battle.Instructions
            };
            return BattleDTO;
        }
        return null!;
    }

    [HttpGet("battlebyid")]
    public async Task<IActionResult> GetBattleById([FromQuery] int battleId, [FromQuery] int chapterId)
    {
        try
        {
            var user = GetUser();

            var chapter = GetChapterById(chapterId);

            var battle = chapter.Battles.FirstOrDefault(b => b.Id == battleId);
            if (battle == null)
                return NotFound("Battle was not found");

            var battleDTO = MapBattleByUserProgress(battle, user);
            return Ok(battleDTO);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChaptersAsync()
    {
        try
        {
            var user = GetUser();

            var chapters = GetChapters();

            var chaptersRequests = MapChaptersByUserProgress(chapters, user);

            return Ok(new ChaptersDTO { ChaptersList = chaptersRequests });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
