namespace CSharp_Interactive_Learning_App.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; }

        public ICollection<int> CompletedLessonIds { get; set; } = [];
        public ICollection<int> UnlockedLessonIds { get; set; } = [1];
    }
}
