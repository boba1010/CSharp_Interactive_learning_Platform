using CSharp_Interactive_Learning_App.Models;

namespace CSharp_Interactive_Learning_App.DTOs.UserDTOs
{
    public class UserLoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    public class UserLoginResponse
    {
        public User User { get; set; }
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Feedback { get; set; } = null!;
    }
}
