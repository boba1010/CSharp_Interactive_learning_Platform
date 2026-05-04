namespace CSharp_Interactive_Learning_App.API.DTOs.BattleDTOs
{
    public class RequestBattleCompletion
    {
        public int BattleId { get; set; }
        public string Token { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class RequestBattleStart
    {
        public string Token { get; set; } = null!;
        public int BattleId { get; set; }
    }

    public class BattleStartResult
    {
        public int Id { get; set; }
        public int PlayerHealth { get; set; }
        public int EnemiesHealth { get; set; }
        public int EnemiesNumber { get; set; }

        public int Turn { get; set; }
        public bool IsFinished { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class RequestBattleEnd
    {
        public int BattleId { get; set; }
        public string Token { get; set; } = null!;
    }

    public class BattleEndResult
    {
        public bool IsSuccess { get; set; }
    }

    public class BattleResult
    {
        public bool IsSuccess { get; set; }
        public bool IsOver { get; set; }
        public DamageCalculationDTO CalculationResult { get; set; }
    }

    public class PreBattleRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Instructions { get; set; } = null!;
        public int EnemiesNumber { get; set; }
        public int HealthPerEnemy { get; set; }

        public bool IsUnlocked { get; set; }
    }
}
