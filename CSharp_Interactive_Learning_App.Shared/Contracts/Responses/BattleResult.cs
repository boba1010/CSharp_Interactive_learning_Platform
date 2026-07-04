using CSharp_Interactive_Learning_App.Shared.DTOs;

namespace CSharp_Interactive_Learning_App.Shared.Contracts.Responses
{
    public class BattleResult
    {
        public bool IsOver { get; set; }
        public int TotalXpGained { get; set; }
        public DamageCalculationDTO CalculationResult { get; set; }
        public int Turn { get; set; }
    }
}
