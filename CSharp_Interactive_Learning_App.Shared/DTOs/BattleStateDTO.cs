namespace CSharp_Interactive_Learning_App.Shared.DTOs
{
    public class BattleStateDTO
    {
        public int BattleStateId { get; set; }
        public int PlayerHealth { get; set; }
        public int EnemiesHealth { get; set; }
        public int EnemiesFullHealth { get; set; }
        public int EnemiesNumber { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }

        public int Turn { get; set; }
        public bool IsBattleOver { get; set; }
    }
}
