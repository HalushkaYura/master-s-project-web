using SmartClass.Application.Contracts.Messages;

namespace SmartClass.Application.Abstractions;

public interface IChatRealtimeSender
{
    Task SendToChannelAsync(Guid channelId, ChatMessageDto dto, CancellationToken ct = default);
}
