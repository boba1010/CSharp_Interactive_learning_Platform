namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public class TokenService(ISecureStorage secureStorage) : ITokenService
    {
        private const string TokenKey = "Token";
        private const string RefreshTokenKey = "RefreshToken";

        public async Task<string?> GetTokenAsync()
        {
            var token = await secureStorage.GetAsync(TokenKey);
            return token;
        }

        public async Task SetTokenAsync(string token)
        {
            await secureStorage.SetAsync(TokenKey, token);
        }

        public async Task SetRefreshTokenAsync(string refreshToken)
        {
            await secureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            var token = await secureStorage.GetAsync(RefreshTokenKey);
            return token;
        }

        public async Task DeleteTokensAsync()
        {
            await secureStorage.DeleteAsync(TokenKey);
            await secureStorage.DeleteAsync(RefreshTokenKey);
        }
    }
}
