namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public interface ITokenService
    {
        public Task<string?> GetTokenAsync();
        public Task<string?> GetRefreshTokenAsync();
        public Task SetTokenAsync(string token);
        public Task SetRefreshTokenAsync(string token);
    }
}
