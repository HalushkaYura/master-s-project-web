using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartClass.Web.Hubs
{
    [Authorize] // якщо в тебе є JWT/кукі автентифікація
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            // тут поки нічого не робимо, підписка на канал буде методами JoinChannel
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Викликається клієнтом, щоб приєднатися до групи певного каналу (класу).
        /// </summary>
        public Task JoinChannel(Guid channelId)
        {
            var groupName = GetGroupName(channelId);
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// За бажанням – метод виходу з каналу.
        /// </summary>
        public Task LeaveChannel(Guid channelId)
        {
            var groupName = GetGroupName(channelId);
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private static string GetGroupName(Guid channelId) => $"channel-{channelId}";
    }
}
