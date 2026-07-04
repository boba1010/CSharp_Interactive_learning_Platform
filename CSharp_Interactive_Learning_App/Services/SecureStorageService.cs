namespace CSharp_Interactive_Learning_App.Services
{
    public class SecureStorageService : Shared.Services.ISecureStorage
    {
        public async Task DeleteAsync(string key)
        {
            SecureStorage.Remove(key);
        }

        public async Task<string?> GetAsync(string key)
        {
            return await SecureStorage.GetAsync(key);
        }

        public async Task SetAsync(string key, string value)
        {
            await SecureStorage.SetAsync(key, value);
        }
        public void Clear()
        {
            SecureStorage.RemoveAll();
        }
    }
}
