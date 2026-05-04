using CSharp_Interactive_Learning_App.API.Models;
using Microsoft.AspNetCore.Identity;

namespace CSharp_Interactive_Learning_App.API.DTOs.UserDTOs
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
        public string Feedback { get; set; } = null!;
    }
}
