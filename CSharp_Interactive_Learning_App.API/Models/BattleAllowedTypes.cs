namespace CSharp_Interactive_Learning_App.API.Models
{
    public class BattleAllowedTypes
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public DataType AllowedType { get; set; }
        public Battle Battle { get; set; } = null!;
    }
}
