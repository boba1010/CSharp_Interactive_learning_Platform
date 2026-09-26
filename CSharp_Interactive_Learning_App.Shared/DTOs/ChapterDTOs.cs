namespace CSharp_Interactive_Learning_App.Shared.DTOs
{
    public class ChaptersDTO
    {
        public List<ChapterDTO> ChaptersList { get; set; } = [];
    }

    public class ChapterDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<BattleDTO> Battles { get; set; } = [];
    }
}
