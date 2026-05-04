namespace CSharp_Interactive_Learning_App.API.DTOs.BattleDTOs
{
    public class DamageCalculationDTO
    {
        public List<EnemyDTO> RemainingEnemies { get; set; } = [];
        public List<string> Errors { get; set; } = [];
        public int SelfDamage { get; set; }
        public int TotalXpGained { get; set; }
    }
}
