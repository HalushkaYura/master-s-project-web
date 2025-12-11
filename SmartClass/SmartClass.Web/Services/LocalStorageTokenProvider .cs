using SmartClass.Application.Abstractions.Auth;

namespace SmartClass.Web.Services
{
    public class LocalStorageTokenProvider : ITokenProvider
    {
        private readonly LocalStorage _localStorage;

        public LocalStorageTokenProvider(LocalStorage localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            // Тут саме "accessToken" – як у тебе в localStorage
            return await _localStorage.GetAsync("accessToken");
        }
    }
}
