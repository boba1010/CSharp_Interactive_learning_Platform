namespace CSharp_Interactive_Learning_App.Shared.Contracts.Responses
{
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
