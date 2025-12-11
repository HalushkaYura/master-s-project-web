namespace SmartClass.Web.Hubs
{
    public interface IChatClient
    {
        Task MessageReceived(ChatMessageDto message);
        // за бажанням:
        // Task UserJoined(string userName);
        // Task UserLeft(string userName);
    }
}
