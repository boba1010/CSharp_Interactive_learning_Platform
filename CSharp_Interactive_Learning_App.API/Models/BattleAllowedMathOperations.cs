namespace CSharp_Interactive_Learning_App.API.Models
{
    public class BattleAllowedMathOperations
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public MathOperationType AllowedMathOperation { get; set; }
        public Battle Battle { get; set; } = null!;
    }
}
