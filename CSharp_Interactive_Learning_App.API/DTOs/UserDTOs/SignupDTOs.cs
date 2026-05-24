using CSharp_Interactive_Learning_App.API.Models;

namespace CSharp_Interactive_Learning_App.API.DTOs.UserDTOs
{
    public class UserSignupResponse
    {
        public UserDTO User { get; set; }
        public string RefreshToken { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Feedback { get; set; } = null!;
    }
    public class UserSignupRequest
    {
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
