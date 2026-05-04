namespace DamageCalculator.Models
{
    public class CalculationResult
    {
        public List<Enemy> RemainingEnemies { get; set; } = [];
        public List<string> Errors { get; set; } = [];
        public int SelfDamage { get; set; }
    }
}
