namespace SmartClass.Web.Services
{
    // Інтерфейс для інжекції
    public interface ILocalizationService
    {
        string Get(string key, params string[] args);
    }
}