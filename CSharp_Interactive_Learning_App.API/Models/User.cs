using Microsoft.EntityFrameworkCore;

namespace CSharp_Interactive_Learning_App.API.Models
{
    public enum DevLevel
    {
        AbsoluteBeginner,
        Beginner,
        Intermediate,
        AdvancedIntermediate,
        Advanced,
    }

    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public byte[] Salt { get; set; }
        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; }
        
        public ICollection<int> CompletedLessonIds { get; set; } = [];
        public ICollection<int> UnlockedLessonIds { get; set; } = [1];
    }
}
