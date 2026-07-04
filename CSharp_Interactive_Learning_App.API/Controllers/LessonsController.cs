using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using CSharp_Interactive_Learning_App.Shared.DTOs;
using DamageCalculatorV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CSharp_Interactive_Learning_App.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chapters")]
    public class LessonsController(AppDbContext dbContext) : ControllerBase
    {
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException();
            return userId;
        }

        private List<ChapterDTO> MapChapters(List<Chapter> chapters, User user)
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

        private BattleDTO MapBattle(Battle battle, User user)
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
            return null;
        }
        
        [HttpGet("battlebyid")]
        public async Task<IActionResult> GetBattleById([FromQuery] int battleId, [FromQuery] int chapterId)
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Forbid();

            var chapter = dbContext.Chapters
                .Include(c => c.Battles)
                .FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                return NotFound("Chapter was not found");

            var battle = chapter.Battles
                .FirstOrDefault(b => b.Id == battleId);
            if (battle == null)
                return NotFound("Battle was not found");
            

            var battleDTO = MapBattle(battle, user);
            return Ok(battleDTO);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChaptersAsync()
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Forbid();

            var chapters = dbContext.Chapters
                .Include(c => c.Battles)
                .ToList();

            var chaptersRequests = MapChapters(chapters, user);

            return Ok(new ChaptersDTO { Chapters = chaptersRequests });
        }

        private DamageCalculationDTO GetCalculationResult(string code, bool isBoss, int enemiesNumber, int healthPerEnemy, double dmgMultiplier)
        {
            DmgCalc dmgCalc = new();
            var result = dmgCalc.Main(code, enemiesNumber, healthPerEnemy, isBoss, dmgMultiplier);

            List<EnemyDTO> enemies = [];
            foreach (var enemy in result.RemainingEnemies)
                enemies.Add(new() { Health = enemy.Health });

            return new()
            {
                Errors = result.Errors,
                RemainingEnemies = enemies,
                DamageTaken = result.SelfDamage,
                //DateTypesUsed = (DataType)result.DateTypesUsed,
                VariableCount = result.VariableCount
            };
        }

        [HttpPost("startBattle")]
        public async Task<IActionResult> RequestBattleStart([FromBody] RequestBattleStart request)
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Forbid();

            var chapter = dbContext.Chapters
                .Include(c => c.Battles)
                .FirstOrDefault(b => b.Id == request.ChapterId);
            if (chapter == null)
                return NotFound("Chapter was not found");

            var battle = chapter.Battles
                .FirstOrDefault(b => b.Id == request.BattleId);
            if (battle == null)
                return NotFound("battle was not found.");

            if (dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id || !b.IsFinished) != null)
                return BadRequest("User is already in a battle.");

            BattleState battleState = new() 
            { 
                UserId = user.Id, 
                LessonId = request.BattleId, 
                IsFinished = false, 
                Turn = 1, 
                PlayerHealth = 100, 
                EnemiesHealth = battle.HealthPerEnemy * battle.EnemiesNumber, 
                EnemiesNumber = battle.EnemiesNumber 
            };

            dbContext.BattleStates.Add(battleState);
            await dbContext.SaveChangesAsync();

            BattleState? savedBattleState = dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id);

            return Ok(new BattleStartResponse 
            { 
                EnemiesHealth = battleState.EnemiesHealth, 
                EnemiesNumber = battleState.EnemiesNumber, 
                EnemiesFullHealth = battleState.EnemiesHealth,
                IsBattleOver = false, 
                PlayerHealth = battleState.PlayerHealth, 
                Turn = 1,
                BattleStateId = savedBattleState.Id
            });
        }

        [HttpPost("validateBattle")]
        public async Task<IActionResult> ValidateBattleState([FromBody] RequestRoundCompletion request)
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Forbid();

            var battleState = dbContext.BattleStates
                .FirstOrDefault(b => b.UserId == user.Id);
            if (battleState == null)
                return NotFound("Battle session could not be found.");

            var chapter = dbContext.Chapters
                .Include(c => c.Battles)
                    .ThenInclude(b => b.AllowedTypes)
                 .Include(c => c.Battles)
                    .ThenInclude(b => b.RequiredStatements)
                .Include(c => c.Battles)
                    .ThenInclude(b => b.AllowedMathOperations)
                .FirstOrDefault(c => c.Id == request.ChapterId);
            if (chapter == null)
                return NotFound("Chapter could not be found");

            var battle = chapter.Battles
                .FirstOrDefault(l => l.Id == battleState.LessonId);
            if (battle == null)
                return NotFound("Battle could not be found.");

            DamageCalculationDTO result;
            
            result = await Task.Run(() => GetCalculationResult(request.Code, false, battleState.EnemiesNumber, battleState.EnemiesHealth / battleState.EnemiesNumber, battle.DmgMultiplier));
            
            battleState.Turn += 1;

            if (result.RemainingEnemies.Count == 0)
            {
                if (!user.CompletedLessonIds.Contains(battleState.LessonId))
                {
                    user.CompletedLessonIds.Add(battle.Id);
                    user.UnlockedLessonIds.Add(battle.Id + 1);
                }

                int questsXp = 4;
                int basexp = 15;
                int xpPerHealth = 2;
                int xpPerEnemy = 5;
                int xpGained = basexp + (battle.EnemiesNumber * xpPerEnemy) + (battle.HealthPerEnemy * battle.EnemiesNumber * xpPerHealth);

                foreach (var statement in battle.RequiredStatements)
                {
                    if (result.VariableCount == statement.Count && 
                        statement.AllowedStatementType == StatementType.Variable)
                    {
                        xpGained += questsXp;
                    }
                }

                user.TotalXp += xpGained;
                dbContext.Users.Update(user);

                dbContext.BattleStates.Remove(battleState);
                
                await dbContext.SaveChangesAsync();
                return Ok(new BattleResult { IsOver = true, TotalXpGained = xpGained, Turn = battleState.Turn });
            }

            battleState.EnemiesNumber = result.RemainingEnemies.Count;
            battleState.EnemiesHealth = result.RemainingEnemies.Sum(e => e.Health);
            battleState.PlayerHealth = battleState.PlayerHealth - result.DamageTaken;
            dbContext.BattleStates.Update(battleState);
            await dbContext.SaveChangesAsync();
            return Ok(new BattleResult { IsOver = false, CalculationResult = result, Turn = battleState.Turn });
        }

        [HttpPost("endBattle")]
        public async Task<IActionResult> RequestBattleEnd([FromBody] RequestBattleEnd request)
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Forbid();

            var battleState = dbContext.BattleStates.FirstOrDefault(b => b.Id == request.BattleSessionId);
            if (battleState == null)
                return NotFound("Battle session could not be found.");

            dbContext.BattleStates.Remove(battleState);
            await dbContext.SaveChangesAsync();
            return Ok(true);
        }
    }
}
