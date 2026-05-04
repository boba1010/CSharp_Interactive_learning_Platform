using System.Collections.ObjectModel;

namespace CSharp_Interactive_Learning_App.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ObservableCollection<Battle> Battles { get; set; } = [];
    }
}
