using DamageCalculatorV2.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace DamageCalculatorV2;

public static class DmgCalc
{
    public static CalculationResult Calculate(string code, int enemiesNumber, int healthPerEnemy, double dmgMultiplier)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        ];

        var compilation = CSharpCompilation.Create("Temp", [syntaxTree], references);

        var errorDmgEvaluator = new ErrorDamageEvaluator(dmgMultiplier, compilation);

        var errors = errorDmgEvaluator.Evaluate();

        var cleanTree = InvalidSyntaxRemover.Filter(syntaxTree);

        var cleanCompilation = CSharpCompilation.Create("Temp", [cleanTree], references);

        var semanticModel = cleanCompilation.GetSemanticModel(cleanTree);
        var dmgEvaluator = new SyntaxDamageEvaluator(dmgMultiplier, semanticModel);
        dmgEvaluator.Visit(cleanTree.GetRoot());

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