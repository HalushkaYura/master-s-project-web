namespace SmartClass.Application.Abstractions.Auth
{
    public interface ITokenProvider
    {
        Task<string?> GetAccessTokenAsync();
    }
}
