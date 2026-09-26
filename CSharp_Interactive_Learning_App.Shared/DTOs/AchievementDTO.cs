namespace CSharp_Interactive_Learning_App.Shared.DTOs;

public sealed class AchievementDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Unlocked { get; set; }
    public int RewardPoints { get; set; }
}
