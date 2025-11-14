using Microsoft.AspNetCore.SignalR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Messages;
using SmartClass.Web.Hubs;

public class ChatHubPublisher : IChatPublisher
{
    private readonly IHubContext<ChatHub> hubContext;

    public ChatHubPublisher(IHubContext<ChatHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task PublishMessageAsync(ChatMessageDto dto, CancellationToken ct)
    {
        await hubContext.Clients
            .Group(dto.ChannelId.ToString())
            .SendAsync("newMessage", dto, ct);
    }
}
