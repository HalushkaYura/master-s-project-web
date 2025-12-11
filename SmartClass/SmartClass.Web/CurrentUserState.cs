using SmartClass.Application.Contracts.Auth;
using SmartClass.Application.Contracts.User;

namespace SmartClass.Web.Services
{
    public class CurrentUserState
    {
        public UserInfoDTO? User { get; private set; }
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }

        // Чи вже виконана початкова перевірка (localStorage / кукі)
        public bool IsInitialized { get; private set; }

        public event Action? OnChange;

        public void SetAuth(TokenPairDto tokens, UserInfoDTO user)
        {
            AccessToken = tokens.AccessToken;
            RefreshToken = tokens.RefreshToken.Token;
            User = user;
            IsInitialized = true;
            NotifyStateChanged();
        }

        public void SetAuth(UserInfoDTO user)
        {
            User = user;
            IsInitialized = true;
            NotifyStateChanged();
        }

        public void Clear()
        {
            User = null;
            AccessToken = null;
            RefreshToken = null;
            IsInitialized = true; // важливо: ми завершили ініціалізацію, юзер = гість
            NotifyStateChanged();
        }

        public void MarkInitialized()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
