using System.Net.Http.Json;
using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Web.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient http;

        public AuthApiClient(HttpClient http)
        {
            this.http = http;
        }

        public async Task<TokenPairDto?> LoginAsync(LoginDto dto)
        {
            var res = await http.PostAsJsonAsync("api/auth/login", dto);
            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<TokenPairDto>();
        }

        public async Task<TokenPairDto?> RegisterAsync(RegisterDto dto)
        {
            var res = await http.PostAsJsonAsync("api/auth/register", dto);
            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<TokenPairDto>();
        }

        public async Task<UserInfoDto?> GetMeAsync()
        {
            return await http.GetFromJsonAsync<UserInfoDto>("api/account/me");
        }
    }
}
