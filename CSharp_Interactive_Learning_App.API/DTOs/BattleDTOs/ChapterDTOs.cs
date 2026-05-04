using System.Collections.ObjectModel;

namespace CSharp_Interactive_Learning_App.API.DTOs.BattleDTOs
{
    public class ChaptersRequest
    {
        public List<ChapterRequest> Chapters { get; set; } = [];
    }

    public class ChapterRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ObservableCollection<PreBattleRequest> Battles { get; set; } = [];
    }
}
