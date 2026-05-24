namespace CSharp_Interactive_Learning_App.API.Models
{
    public class BattleRequiredStatement
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public StatementType AllowedStatementType { get; set; }
        public int Count { get; set; }
        public Battle Battle { get; set; } = null!;
    }
}
