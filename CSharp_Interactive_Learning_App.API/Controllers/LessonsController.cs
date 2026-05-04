using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.DTOs.BattleDTOs;
using CSharp_Interactive_Learning_App.API.Models;
using DamageCalculatorV2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CSharp_Interactive_Learning_App.API.Controllers
{
    [ApiController]
    [Route("api/chapters")]
    public class LessonsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> GetAllChaptersAsync([FromBody] string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "CSharp_Interactive_Learning_App.API",

                ValidateAudience = true,
                ValidAudience = "CSharp_Interactive_Learning_App",

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=3cm9d+5r+4=cd4+dc4=dde=dcc23gjjtygv"))
            };

            var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

            int userId = Convert.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var chapters = dbContext.Chapters.ToList();
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return BadRequest("User was not found.");

            List<ChapterRequest> chaptersRequests = [];

            foreach (var chapter in chapters)
            {
                ObservableCollection<PreBattleRequest> preBattles = [];
                foreach (var battleId in chapter.LessonIds)
                {
                    var battle = dbContext.Battles.FirstOrDefault(b => b.Id == battleId);
                    if (battle == null)
                        break;
                    PreBattleRequest preBattleRequest = new() 
                    { 
                        Content = battle.Content, 
                        Id = battle.Id, 
                        Name = battle.Name, 
                        EnemiesNumber = battle.EnemiesNumber, 
                        HealthPerEnemy = battle.HealthPerEnemy, 
                        Instructions = battle.Instructions 
                    };
                    if (!user.UnlockedLessonIds.Contains(battleId))
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

            var enemies = new List<EnemyDTO>();
            foreach (var enemy in result.RemainingEnemies)
                enemies.Add(new() { Health = enemy.Health });

            return new() { Errors = result.Errors, RemainingEnemies = enemies, SelfDamage = result.SelfDamage, };
        }

        [HttpPost("startBattle")]
        public async Task<IActionResult> RequestBattleStart([FromBody] RequestBattleStart request)
        {
            var handler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "CSharp_Interactive_Learning_App.API",

                ValidateAudience = true,
                ValidAudience = "CSharp_Interactive_Learning_App",

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=3cm9d+5r+4=cd4+dc4=dde=dcc23gjjtygv"))
            };

            var principal = handler.ValidateToken(request.Token, validationParams, out var validatedToken);

            int userId = Convert.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User was not found.");

            var battle = dbContext.Battles.FirstOrDefault(b => b.Id == request.BattleId);
            if (battle == null)
                return NotFound("Lesson was not found.");

            if (dbContext.BattleStates.FirstOrDefault(b => b.UserId == userId) != null)
                return BadRequest("User is already in a battle.");

            BattleState battleState = new() 
            { 
                UserId = userId, 
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
            var handler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "CSharp_Interactive_Learning_App.API",

                ValidateAudience = true,
                ValidAudience = "CSharp_Interactive_Learning_App",

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=3cm9d+5r+4=cd4+dc4=dde=dcc23gjjtygv"))
            };

            var principal = handler.ValidateToken(request.Token, validationParams, out var validatedToken);

            int userId = Convert.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User was not found.");

            var battleState = dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id);
            if (battleState == null)
                return NotFound("Battle session could not be found.");

            var battle = dbContext.Battles.FirstOrDefault(l => l.Id == battleState.LessonId);
            if (battle == null)
                return NotFound("Lesson could not be found.");

            DamageCalculationDTO result;
            
            result = await Task.Run(() => GetCalculationResult(request.Code, false, battleState.EnemiesNumber, battleState.EnemiesHealth / battleState.EnemiesNumber, battle.DmgMultiplier));

            if (result.RemainingEnemies.Count == 0)
            {
                if (!user.CompletedLessonIds.Contains(battleState.LessonId))
                {
                    user.CompletedLessonIds.Add(battle.Id);
                    user.UnlockedLessonIds.Add(battle.Id + 1);
                }

                int basexp = 15;
                int xpPerHealth = 2;
                int xpPerEnemy = 5;
                int xpGained = basexp + (battle.EnemiesNumber * xpPerEnemy) + (battle.HealthPerEnemy * battle.EnemiesNumber * xpPerHealth);
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
            var handler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "CSharp_Interactive_Learning_App.API",

                ValidateAudience = true,
                ValidAudience = "CSharp_Interactive_Learning_App",

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=3cm9d+5r+4=cd4+dc4=dde=dcc23gjjtygv"))
            };

            var principal = handler.ValidateToken(request.Token, validationParams, out var validatedToken);

            int userId = Convert.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

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
