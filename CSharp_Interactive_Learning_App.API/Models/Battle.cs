namespace CSharp_Interactive_Learning_App.API.Models
{
    public class Battle
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Instructions { get; set; } = null!;
        public int EnemiesNumber { get; set; }
        public int HealthPerEnemy { get; set; }
        public double DmgMultiplier { get; set; }
    }
}
