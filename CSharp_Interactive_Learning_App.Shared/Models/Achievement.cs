namespace CSharp_Interactive_Learning_App.Shared.Models;

public sealed class Achievement
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int RewardPoints { get; set; }
    public bool Unlocked { get; set; }
    public bool Locked { get; set; }
}
