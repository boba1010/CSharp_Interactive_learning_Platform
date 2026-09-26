using DamageCalculatorV2.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace DamageCalculatorV2;

public class DmgCalc
{
    public CalculationResult Calculate(string code, int enemiesNumber, int healthPerEnemy, bool isBoss, double dmgMultiplier)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var errorDmgEvaluator = new ErrorDamageEvaluator(dmgMultiplier);

        var errors = errorDmgEvaluator.Evaluate(syntaxTree);

        var root = syntaxTree.GetCompilationUnitRoot();

        var cleanRoot = InvalidSyntaxRemover.Filter(root);

        var dmgEvaluator = new SyntaxDamageEvaluator(dmgMultiplier);
        dmgEvaluator.Visit(cleanRoot);

        var totalDamageDealt = dmgEvaluator.DamageDealt;

        List<Enemy> enemies = [];
        for (int i = 0; i < enemiesNumber; i++)
        {
            var enemy = new Enemy() { Health = (int)Math.Round(healthPerEnemy - totalDamageDealt) };
            
            totalDamageDealt -= healthPerEnemy;
            
            if (totalDamageDealt < 0)
                totalDamageDealt = 0;

            if (enemy.Health <= 0)
                continue;

            enemies.Add(enemy);
        }

        return new()
        {
            Errors = errors,
            RemainingEnemies = enemies,
            DamageTaken = (int)Math.Round(errorDmgEvaluator.DamageTaken),
            DamageDealt = (int)Math.Round(dmgEvaluator.DamageDealt),
            Statements = dmgEvaluator.Statements,
        };
    }
}