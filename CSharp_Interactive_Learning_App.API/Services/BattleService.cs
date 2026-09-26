using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Contracts.Responses;
using CSharp_Interactive_Learning_App.Shared.DTOs;
using CSharp_Interactive_Learning_App.Shared.Enums;
using DamageCalculatorV2;
using DamageCalculatorV2.Nodes;
using Microsoft.EntityFrameworkCore;

namespace CSharp_Interactive_Learning_App.API.Services;

public class BattleService(AppDbContext dbContext) : IBattleService
{
    public async Task EndBattleAsync(RequestBattleEnd request)
    {
        var battleState = dbContext.BattleStates.FirstOrDefault(b => b.Id == request.BattleSessionId)
            ?? throw new InvalidOperationException("Battle session could not be found.");

        dbContext.BattleStates.Remove(battleState);
        await dbContext.SaveChangesAsync();
    }

    public async Task<BattleStartResponse> StartBattleAsync(RequestBattleStart request, User user)
    {
        var chapter = dbContext.Chapters
            .Include(c => c.Battles)
            .FirstOrDefault(b => b.Id == request.ChapterId)
            ?? throw new InvalidOperationException("Chapter was not found");

        var battle = chapter.Battles.FirstOrDefault(b => b.Id == request.BattleId)
            ?? throw new InvalidOperationException("battle was not found.");

        if (dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id) != null)
            throw new InvalidOperationException("User is already in a battle.");

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

        return new BattleStartResponse
        {
            EnemiesHealth = battleState.EnemiesHealth,
            EnemiesNumber = battleState.EnemiesNumber,
            EnemiesFullHealth = battleState.EnemiesHealth,
            IsBattleOver = false,
            PlayerHealth = battleState.PlayerHealth,
            Turn = 1,
            BattleStateId = battleState.Id
        };
    }

    public async Task<BattleResult> ValidateBattleStep(RequestRoundCompletion request, User user)
    {
        var battleState = dbContext.BattleStates.FirstOrDefault(b => b.UserId == user.Id) 
            ?? throw new InvalidOperationException("Battle session could not be found.");
        var chapter = GetChapterById(request.ChapterId);

        var battle = chapter.Battles.FirstOrDefault(b => b.Id == battleState.LessonId) 
            ?? throw new InvalidOperationException("Battle could not be found.");

        DamageCalculationDTO result;

        result = GetCalculationResult(request.Code, false, battleState.EnemiesNumber, battleState.EnemiesHealth / battleState.EnemiesNumber, battle.DmgMultiplier);

        battleState.Turn += 1;

        if (result.RemainingEnemies.Count == 0)
        {
            int questsXp = 5;
            int xpGained = CalculateXp(battle.EnemiesNumber, battle.HealthPerEnemy);

            var (variableDeclarations, variableAssignments) = GetVariableStatements(result.Statements);

            foreach (var declaration in variableDeclarations)
            {
                if (!battle.AllowedTypesSet.Contains(declaration.Type))
                {
                    await dbContext.SaveChangesAsync();
                    return new BattleResult
                    {
                        Turn = battleState.Turn,
                        CalculationResult = result,
                        Feedback = "Illegal DataType Used"
                    };
                }
                else
                    xpGained += questsXp;

                if (declaration.Value == null)
                    continue;

                var op = (OperationType)declaration.Value.Operator;
                if (!battle.AllowedOperationsSet.Contains(op))
                {
                    await dbContext.SaveChangesAsync();
                    return new BattleResult
                    {
                        Turn = battleState.Turn,
                        CalculationResult = result,
                        Feedback = "Illegal Operation Used"
                    };
                }
                else
                    xpGained += questsXp;
            }

            foreach (var assignment in variableAssignments)
            {
                var op = (OperationType)assignment.Value.Operator;
                if (!battle.AllowedOperationsSet.Contains(op))
                {
                    await dbContext.SaveChangesAsync();
                    return new BattleResult
                    {
                        Turn = battleState.Turn,
                        CalculationResult = result,
                        Feedback = "Illegal Operation Used"
                    };
                }
                else
                    xpGained += questsXp;
            }

            foreach (var statement in battle.RequiredStatements)
            {
                switch (statement.AllowedStatementType)
                {
                    case StatementType.VariableDeclaration:
                        if (variableDeclarations.Count >= statement.Count)
                            xpGained += questsXp;
                        break;
                    case StatementType.VariableAssignment:
                        if (variableAssignments.Count >= statement.Count)
                            xpGained += questsXp;
                        break;
                }
            }

            user.TotalXp += xpGained;
            user.CurrentLevel = CalculateLevel(user.TotalXp);

            if (!user.CompletedLessonIds.Contains(battleState.LessonId))
            {
                user.CompletedLessonIds.Add(battle.Id);
                user.UnlockedLessonIds.Add(battle.Id + 1);
            }

            user.TotalPoints += battle.Points;
            dbContext.Users.Update(user);

            dbContext.BattleStates.Remove(battleState);

            await dbContext.SaveChangesAsync();
            return new BattleResult 
            { 
                IsOver = true, 
                TotalXpGained = xpGained,
                Turn = battleState.Turn,
                CalculationResult = result 
            };
        }

        battleState.EnemiesNumber = result.RemainingEnemies.Count;
        battleState.EnemiesHealth = result.RemainingEnemies.Sum(e => e.Health);
        battleState.PlayerHealth -= result.DamageTaken;
        dbContext.BattleStates.Update(battleState);
        await dbContext.SaveChangesAsync();

        return new BattleResult 
        { 
            CalculationResult = result,
            Turn = battleState.Turn,
        };
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

    private DamageCalculationDTO GetCalculationResult(string code, bool isBoss, int enemiesNumber, int healthPerEnemy, double dmgMultiplier)
    {
        DmgCalc dmgCalc = new();
        var result = dmgCalc.Calculate(code, enemiesNumber, healthPerEnemy, isBoss, dmgMultiplier);

        List<EnemyDTO> enemies = [];
        foreach (var enemy in result.RemainingEnemies)
            enemies.Add(new() { Health = enemy.Health });

        static ExpressionDTO? MapExpression(Expression? expression) => expression is null
            ? null
            : new(expression.Left, expression.Right, (SyntaxKind)expression.Operator);

        List<StatementDTO> statements =
        [
            .. result.Statements.Select(statement => statement switch
                {
                    VariableDeclarationStatement v => (StatementDTO)new VariableDeclarationDTO((DataType)v.Type, v.Name, MapExpression(v.Value)),
                    VariableAssignmentStatement v => new VariableAssignmentDTO(v.Name, MapExpression(v.Value)!),
                    _ => throw new NotSupportedException($"Unsupported statement type: {statement.GetType().Name}")
                })
        ];

        return new()
        {
            Errors = result.Errors,
            RemainingEnemies = enemies,
            DamageTaken = result.DamageTaken,
            DamageDealt = result.DamageDealt,
            Statements = statements,
        };
    }

    private static int CalculateXp(int enemiesNumber, int healthPerEnemy)
    {
        int basexp = 15;
        int xpPerHealth = 2;
        int xpPerEnemy = 5;
        int xpGained = basexp + (enemiesNumber * xpPerEnemy) + (healthPerEnemy * enemiesNumber * xpPerHealth);

        return xpGained;
    }

    private (List<VariableDeclarationDTO> variableDeclarations, List<VariableAssignmentDTO> variableAssignments) GetVariableStatements(List<StatementDTO> statements)
    {
        List<VariableDeclarationDTO> variableDeclarations = [];
        List<VariableAssignmentDTO> variableAssignments = [];

        foreach (var statement in statements)
        {
            if (statement is VariableDeclarationDTO variableDeclaration)
                variableDeclarations.Add(variableDeclaration);
            else if (statement is VariableAssignmentDTO variableAssignment)
                variableAssignments.Add(variableAssignment);
        }

        return (variableDeclarations, variableAssignments);
    }

    private int CalculateLevel(int xp)
    {
        int devLevel = 0;

        List<DeveloperLevel> levels = [.. dbContext.DeveloperLevels];
        foreach (var level in levels)
        {
            if (xp >= level.RequiredXp)
            {
                xp -= level.RequiredXp;
                devLevel = level.Level;
            }
        }

        return devLevel;
    }
}
