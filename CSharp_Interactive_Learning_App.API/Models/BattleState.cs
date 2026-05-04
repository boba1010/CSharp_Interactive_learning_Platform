namespace CSharp_Interactive_Learning_App.API.Models
{
    public class BattleState
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LessonId { get; set; }

        public int PlayerHealth { get; set; }
        public int EnemiesHealth { get; set; }
        public int EnemiesNumber { get; set; }

        public int Turn { get; set; }
        public bool IsFinished { get; set; }
    }
}
