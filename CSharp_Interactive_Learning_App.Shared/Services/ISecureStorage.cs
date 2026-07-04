namespace CSharp_Interactive_Learning_App.Shared.Services
{
    public interface ISecureStorage
    {
        public Task SetAsync(string key, string value);
        public Task<string?> GetAsync(string key);
        public Task DeleteAsync(string key);
        public void Clear();
    }
}
