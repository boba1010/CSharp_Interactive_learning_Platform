namespace CSharp_Interactive_Learning_App.Models
{
    public class Battle
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }

        public string Name { get; set; } = null!;
        public bool IsUnlocked { get; set; }
        public string Content { get; set; } = null!;
        public string Instructions { get; set; } = null!;
        public int EnemiesNumber { get; set; }
        public int HealthPerEnemy { get; set; }
        public int DmgMultipler { get; set; }
    }
}
