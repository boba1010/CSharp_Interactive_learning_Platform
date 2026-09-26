using CSharp_Interactive_Learning_App.API.Enums;

namespace CSharp_Interactive_Learning_App.API.Models;

public sealed class DeveloperLevel(int level, int requiredXp, DevLevel devLevel)
{
    public int Id { get; set; }
    public int Level { get; init; } = level;
    public int RequiredXp { get; init; } = requiredXp;
    public DevLevel DevLevel { get; init; } = devLevel;
}
