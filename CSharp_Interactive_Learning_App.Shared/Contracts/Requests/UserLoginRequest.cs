namespace CSharp_Interactive_Learning_App.Shared.Contracts.Requests
{
    public class UserLoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
