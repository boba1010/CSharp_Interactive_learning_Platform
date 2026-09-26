using DamageCalculatorV2.Nodes;

namespace DamageCalculatorV2.Models;

public class CalculationResult
{
    public List<Enemy> RemainingEnemies { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public List<Statement> Statements { get; set; } = [];
    public int DamageTaken { get; set; }
    public int DamageDealt { get; set; }
}
