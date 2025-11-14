namespace SmartClass.Web.Services;

public class DevJwtState
{
    public string? Token { get; private set; }
    public event Action? Changed;

    public void SetToken(string? token)
    {
        Token = token;
        Changed?.Invoke();
    }
}
