using Microsoft.AspNetCore.SignalR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Messages;
using SmartClass.Web.Hubs;

namespace SmartClass.Web.Services;

public sealed class ChatRealtimeSender : IChatRealtimeSender
{
    private readonly IHubContext<ChatHub> hub;

    public ChatRealtimeSender(IHubContext<ChatHub> hub)
    {
        this.hub = hub;
    }

    public async Task SendToChannelAsync(Guid channelId, ChatMessageDto dto, CancellationToken ct = default)
    {
        await hub.Clients.Group(channelId.ToString())
            .SendAsync("newMessage", dto, ct);
    }
}
