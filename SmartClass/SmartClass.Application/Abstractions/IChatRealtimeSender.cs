
namespace SmartClass.Application.Abstractions
{
    public interface IChatRealtimeSender
    {
        Task SendToChannelAsync(Guid channelId, ChatMessageDto message, CancellationToken ct = default);
    }
}
