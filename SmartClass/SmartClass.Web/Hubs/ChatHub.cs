using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 🔹

using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;
using System.Security.Claims;

namespace SmartClass.Web.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class ChatHub : Hub<IChatClient>
    {
        private readonly IRepository<ChannelMember> channelMembersRepo;
        private readonly IRepository<Message> messagesRepo;

        public ChatHub(
            IRepository<ChannelMember> channelMembersRepo,
            IRepository<Message> messagesRepo)
        {
            this.channelMembersRepo = channelMembersRepo;
            this.messagesRepo = messagesRepo;
        }
        public async Task SendMessage(Guid channelId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var userId = GetUserId();

            var member = await channelMembersRepo.GetEntityAsync(
                m => m.ChannelId == channelId && m.UserId == userId);

            if (member == null)
                throw new HubException("You are not a member of this channel.");

            text = text.Trim();

            var msg = new Message
            {
                ChannelId = channelId,
                AuthorId = userId,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            await messagesRepo.AddAsync(msg);
            await messagesRepo.SaveChangesAsync();

            var dto = new ChatMessageDto(
                msg.Id,
                msg.ChannelId,
                msg.AuthorId,
                GetUserDisplayName(),
                msg.Text,
                msg.CreatedAt
            );

            await Clients.Group(ChannelGroup(channelId))
                         .MessageReceived(dto);
        }
        private Guid GetUserId()
        {
            var sub =
                Context.User?.FindFirst("sub") ??
                Context.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (sub == null || !Guid.TryParse(sub.Value, out var id))
                throw new HubException("Invalid user id");

            return id;
        }

        private string GetUserDisplayName()
            => Context.User?.Identity?.Name ?? "Користувач";

        private static string ChannelGroup(Guid channelId)
            => $"channel-{channelId:N}";

        public async Task JoinChannel(Guid channelId)
        {
            var userId = GetUserId();

            var member = await channelMembersRepo.GetEntityAsync(
                m => m.ChannelId == channelId && m.UserId == userId);

            if (member == null)
                throw new HubException("You are not a member of this channel.");

            await Groups.AddToGroupAsync(Context.ConnectionId, ChannelGroup(channelId));
        }

        public Task LeaveChannel(Guid channelId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, ChannelGroup(channelId));


    }
}
