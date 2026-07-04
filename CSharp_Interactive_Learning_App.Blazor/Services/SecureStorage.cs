using CSharp_Interactive_Learning_App.Shared.Services;

namespace CSharp_Interactive_Learning_App.Blazor.Services
{
    public class SecureStorage(ILocalStorage localStorage) : ISecureStorage
    {
        public async Task DeleteAsync(string key)
        {
            await localStorage.RemoveItem(key);
        }

        public async Task<string?> GetAsync(string key)
        {
            var result = await localStorage.GetItem(key);
            return result;
        }

        public async Task SetAsync(string key, string value)
        {
            await localStorage.SetItem(key, value);
        }

        public void Clear()
        {
        }
    }
}
