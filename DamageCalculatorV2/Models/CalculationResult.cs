namespace DamageCalculatorV2.Models
{
    public class CalculationResult
    {
        public List<Enemy> RemainingEnemies { get; set; } = [];
        public List<string> Errors { get; set; } = [];
        public int SelfDamage { get; set; }

        public int VariableCount { get; set; }
        public List<DataType> DateTypesUsed { get; set; } = [];
    }
}
