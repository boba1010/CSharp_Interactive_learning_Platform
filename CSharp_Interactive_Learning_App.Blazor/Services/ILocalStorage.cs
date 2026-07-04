namespace CSharp_Interactive_Learning_App.Blazor.Services
{
    public interface ILocalStorage
    {
        public Task SetItem(string key, string content);
        public Task<string> GetItem(string key);
        public Task RemoveItem(string key);
    }
}
