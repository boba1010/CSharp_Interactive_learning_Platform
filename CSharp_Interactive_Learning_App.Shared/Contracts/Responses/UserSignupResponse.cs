using CSharp_Interactive_Learning_App.Shared.DTOs;

namespace CSharp_Interactive_Learning_App.Shared.Contracts.Responses
{
    public class UserSignupResponse
    {
        public UserDTO User { get; set; }
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
