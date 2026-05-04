using System.Collections.ObjectModel;

namespace CSharp_Interactive_Learning_App.API.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<int> LessonIds { get; set; } = [];
    }
}
