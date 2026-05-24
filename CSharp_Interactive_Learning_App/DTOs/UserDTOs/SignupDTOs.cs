using CSharp_Interactive_Learning_App.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Interactive_Learning_App.DTOs.UserDTOs
{
    public class UserSignupResponse
    {
        public User User { get; set; }
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
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
