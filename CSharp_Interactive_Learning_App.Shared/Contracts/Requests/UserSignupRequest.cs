namespace CSharp_Interactive_Learning_App.Shared.Contracts.Requests
{
    public class UserSignupRequest
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
