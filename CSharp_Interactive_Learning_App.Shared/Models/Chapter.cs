namespace CSharp_Interactive_Learning_App.Shared.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<Battle> Battles { get; set; } = [];
    }
}
