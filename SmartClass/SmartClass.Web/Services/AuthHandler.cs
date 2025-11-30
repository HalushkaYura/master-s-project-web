using System.Net.Http.Headers;

namespace SmartClass.Web.Services
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly LocalStorage storage;

        public AuthHandler(LocalStorage storage)
        {
            this.storage = storage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await storage.GetAsync("accessToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }

    }
}
