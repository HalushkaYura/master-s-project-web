using Microsoft.JSInterop;

namespace SmartClass.Web.Services
{
    public class LocalStorage
    {
        private readonly IJSRuntime js;

        public LocalStorage(IJSRuntime js)
        {
            this.js = js;
        }

        public ValueTask SetAsync(string key, string value)
            => js.InvokeVoidAsync("localStorage.setItem", key, value);

        public ValueTask<string?> GetAsync(string key)
            => js.InvokeAsync<string?>("localStorage.getItem", key);

        public ValueTask RemoveAsync(string key)
            => js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
