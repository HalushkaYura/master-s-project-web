using SmartClass.Application.Contracts.Messages;

public interface IChatPublisher
{
    Task PublishMessageAsync(ChatMessageDto dto, CancellationToken ct);
}
