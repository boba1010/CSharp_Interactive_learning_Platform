namespace CSharp_Interactive_Learning_App.API.Models
{
    public enum StatementType
    {
        Variable,
    }

    public enum DataType
    {
        Int,
        Float,
        Double,
        String,
        Char,
        Bool,
    }

    public enum MathOperationType
    {
        Addition,
        Subtraction,
        Division,
        Multiplication,
    }

    public class Battle
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }

        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Instructions { get; set; } = null!;
        public int EnemiesNumber { get; set; }
        public int HealthPerEnemy { get; set; }
        public double DmgMultiplier { get; set; }

        public List<BattleAllowedTypes> AllowedTypes { get; set; } = [];
        public List<BattleRequiredStatement> RequiredStatements { get; set; } = null!;
        public List<BattleAllowedMathOperations> AllowedMathOperations { get; set; } = [];
    }
}
