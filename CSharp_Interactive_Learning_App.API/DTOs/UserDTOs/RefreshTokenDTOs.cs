
namespace CSharp_Interactive_Learning_App.API.DTOs.UserDTOs
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; } = null!;
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
