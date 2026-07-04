using Microsoft.JSInterop;

namespace CSharp_Interactive_Learning_App.Blazor.Services
{
    public class LocalStorage(IJSRuntime js) : ILocalStorage
    {
        public async Task SetItem(string key, string content)
        {
            await js.InvokeVoidAsync("localStorage.setItem", key, content);
        }

        public async Task<string> GetItem(string key)
        {
            return await js.InvokeAsync<string>("localStorage.getItem", key);
        }

        public async Task RemoveItem(string key)
        {
            await js.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
