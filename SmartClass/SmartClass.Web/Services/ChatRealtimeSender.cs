using Microsoft.AspNetCore.SignalR;
using SmartClass.Application.Abstractions;
using SmartClass.Web.Hubs;

namespace SmartClass.Web.Services
{
    public sealed class ChatRealtimeSender : IChatRealtimeSender
    {
        private readonly IHubContext<ChatHub> hubContext;

        public ChatRealtimeSender(IHubContext<ChatHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public Task SendToChannelAsync(Guid channelId, ChatMessageDto message, CancellationToken ct = default)
        {
            var groupName = $"channel-{channelId}";
            // "ReceiveMessage" – ім'я методу на клієнті
            return hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message, ct);
        }
    }
}
