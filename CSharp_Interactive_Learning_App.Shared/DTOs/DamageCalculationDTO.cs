namespace CSharp_Interactive_Learning_App.Shared.DTOs;

public class DamageCalculationDTO
{
    public List<EnemyDTO> RemainingEnemies { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public List<StatementDTO> Statements { get; set; } = [];
    public int DamageTaken { get; set; }
    public int DamageDealt { get; set; }
}
