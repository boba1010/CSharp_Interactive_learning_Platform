namespace CSharp_Interactive_Learning_App.Shared.Contracts.Requests
{
    public class RequestRoundCompletion
    {
        public int BattleId { get; set; }
        public int ChapterId { get; set; }
        public string Code { get; set; } = null!;
    }
}
