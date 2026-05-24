using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.DTOs.BattleDTOs;
using CSharp_Interactive_Learning_App.API.Models;
using DamageCalculatorV2;
//using DamageCalculatorV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
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

        [HttpGet]
        public async Task<IActionResult> GetAllChaptersAsync()
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User was not found.");

            var chapters = dbContext.Chapters
                .Include(c => c.Battles)
                .ToList();

            List<ChapterRequest> chaptersRequests = [];

            foreach (var chapter in chapters)
            {
                ObservableCollection<PreBattleRequest> preBattles = [];
                foreach (var battle in chapter.Battles)
                {
                    PreBattleRequest preBattleRequest = new() 
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

            return Ok(new ChaptersRequest { Chapters = chaptersRequests });
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
                SelfDamage = result.SelfDamage,
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
                return NotFound("User was not found.");

            var chapter = dbContext.Chapters
                .Include(c => c.Battles)
                .FirstOrDefault(b => b.Id == request.ChapterId);
            if (chapter == null)
                return NotFound("Chapter was not found");

            var battle = chapter.Battles
                .FirstOrDefault(b => b.Id == request.BattleId);
            if (battle == null)
                return NotFound("battle was not found.");

            if (dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id) != null)
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

            BattleState? battleState_ = dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id);

            return Ok(new BattleStartResult 
            { 
                IsSuccess = true, 
                EnemiesHealth = battleState.EnemiesHealth, 
                EnemiesNumber = battleState.EnemiesNumber, 
                IsFinished = false, 
                PlayerHealth = battleState.PlayerHealth, 
                Turn = 1,
                Id = battleState_.Id
            });
        }

        [HttpPost("validateBattle")]
        public async Task<IActionResult> RequestLessonAndBattleCompletion([FromBody] RequestBattleCompletion request)
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User could not be found.");

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
                result.TotalXpGained = xpGained;
                return Ok(new BattleResult { IsOver = true , CalculationResult = result, IsSuccess = true });
            }

            battleState.Turn += 1;
            battleState.EnemiesNumber = result.RemainingEnemies.Count;
            battleState.EnemiesHealth = result.RemainingEnemies.Sum(e => e.Health);
            battleState.PlayerHealth = battleState.PlayerHealth - result.SelfDamage;
            dbContext.BattleStates.Update(battleState);
            await dbContext.SaveChangesAsync();
            return Ok(new BattleResult { IsOver = false, CalculationResult = result, IsSuccess = true });
        }

        [HttpPost("endBattle")]
        public async Task<IActionResult> RequestBattleEnd([FromBody] RequestBattleEnd request)
        {
            int userId = GetUserId();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User was not found.");

            var battleState = dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id && b.Id == request.BattleId);
            if (battleState == null)
                return NotFound("Battle session could not be found.");

            dbContext.BattleStates.Remove(battleState);
            await dbContext.SaveChangesAsync();
            return Ok(new BattleEndResult { IsSuccess = true });
        }
    }
}
