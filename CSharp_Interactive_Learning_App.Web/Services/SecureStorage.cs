using CSharp_Interactive_Learning_App.Shared.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CSharp_Interactive_Learning_App.Web.Services
{
    public class SecureStorage(ProtectedLocalStorage protectedLocalStorage) : ISecureStorage
    {
        public async Task DeleteAsync(string key)
        {
            await protectedLocalStorage.DeleteAsync(key);
        }

        public async Task<string?> GetAsync(string key)
        {
            var result = await protectedLocalStorage.GetAsync<string?>(key);
            return result.Value;
        }

        public async Task SetAsync(string key, string value)
        {
            await protectedLocalStorage.SetAsync(key, value);
        }

        public void Clear()
        {
        }
    }
}
