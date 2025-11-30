using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Web.Services
{
    public class CurrentUserState
    {
        public UserInfoDto? User { get; private set; }
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }

        public event Action? OnChange;

        public void SetAuth(TokenPairDto tokens, UserInfoDto user)
        {
            AccessToken = tokens.AccessToken;
            RefreshToken = tokens.RefreshToken.Token;
            User = user;
            OnChange?.Invoke();
            NotifyStateChanged();
        }

        public void Clear()
        {
            User = null;
            AccessToken = null;
            RefreshToken = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
