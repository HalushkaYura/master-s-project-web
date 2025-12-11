using SmartClass.Application.Contracts.Auth;
using SmartClass.Application.Contracts.User;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartClass.Web.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetBearer(string? accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        public async Task<TokenPairDto?> LoginAsync(LoginDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadFromJsonAsync<TokenPairDto>();
        }

        public async Task<TokenPairDto?> RegisterAsync(RegisterDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadFromJsonAsync<TokenPairDto>();
        }

        public async Task<UserInfoDTO?> GetCurrentUserAsync()
        {
            // маєш зробити відповідний API-метод на бекенді (наприклад /api/user/me)
            var response = await _httpClient.GetAsync("api/user/info");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserInfoDTO>();
        }

        // за бажанням можна додати RefreshAsync і LogoutAsync

        // Add this method to fix CS1061
        public async Task LogoutAsync(LogoutDto dto)
        {
            await _httpClient.PostAsJsonAsync("api/auth/logout", dto);
        }
    }
}
