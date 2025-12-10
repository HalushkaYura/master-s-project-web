using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Notifications;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications)
        {
            _notifications = notifications;
        }

        private Guid GetUserId()
        {
            var sub =
                User.FindFirst("sub") ??
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (sub == null || !Guid.TryParse(sub.Value, out var id))
                throw new InvalidOperationException("Invalid user id in token.");

            return id;
        }

        /// <summary>
        /// Список сповіщень поточного користувача.
        /// /api/notifications?onlyUnread=true
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get(
            [FromQuery] bool onlyUnread = false,
            CancellationToken ct = default)
        {
            var userId = GetUserId();

            var list = await _notifications.GetForUserAsync(userId, onlyUnread, ct);
            return Ok(list);
        }

        /// <summary>
        /// Позначити конкретну нотифікацію як прочитану.
        /// </summary>
        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
        {
            var userId = GetUserId();

            await _notifications.MarkAsReadAsync(id, userId, ct);
            return NoContent();
        }

        /// <summary>
        /// Позначити всі нотифікації користувача як прочитані.
        /// </summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
        {
            var userId = GetUserId();

            await _notifications.MarkAllAsReadAsync(userId, ct);
            return NoContent();
        }
    }
}
