namespace CSharp_Interactive_Learning_App.Shared.Contracts.Responses;

public class BattleStartResponse
{
    public int BattleStateId { get; set; }
    public int PlayerHealth { get; set; }
    public int EnemiesHealth { get; set; }
    public int EnemiesFullHealth { get; set; }
    public int EnemiesNumber { get; set; }

    public int Turn { get; set; }
    public bool IsBattleOver { get; set; }
}
