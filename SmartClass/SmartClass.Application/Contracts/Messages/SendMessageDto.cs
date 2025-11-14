namespace SmartClass.Application.Contracts.Messages;

public sealed class SendMessageDto
{
    public Guid ChannelId { get; set; }
    public string Text { get; set; } = string.Empty;
}
