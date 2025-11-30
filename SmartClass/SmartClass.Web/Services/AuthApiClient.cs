using System.Net.Http.Headers;
using System.Net.Http.Json;
using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Web.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<TokenPairDto?> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            // POST https://{BaseAddress}/api/Auth/register
            var response = await httpClient.PostAsJsonAsync("api/Auth/register", dto, ct);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<TokenPairDto>(cancellationToken: ct);
        }

        public async Task<TokenPairDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("api/Auth/login", dto, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct);
                // тут API повертає { "message": "Invalid login or password." }
                throw new InvalidOperationException(errorText);
            }

            return await response.Content.ReadFromJsonAsync<TokenPairDto>(cancellationToken: ct);
        }



        public async Task<TokenPairDto?> RefreshAsync(RefreshDto dto, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("api/Auth/refresh", dto, ct);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<TokenPairDto>(cancellationToken: ct);
        }

        /// <summary>
        /// Отримання поточного користувача (треба реалізувати ендпойнт /api/Auth/me)
        /// </summary>
        public async Task<UserInfoDto?> GetCurrentUserAsync(CancellationToken ct = default)
        {
            var response = await httpClient.GetAsync("api/User/info", ct);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<UserInfoDto>(cancellationToken: ct);
        }

        public void SetBearer(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
