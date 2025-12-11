using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Notifications;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services
{
    public sealed class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notifications;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public NotificationService(
            IRepository<Notification> notifications,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            _notifications = notifications;
            _dbFactory = dbFactory;
        }

        public async Task CreateAsync(CreateNotificationDto dto, CancellationToken ct = default)
        {
            var entity = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Type = dto.Type,
                PayloadJson = dto.PayloadJson,
                IsRead = false
            };

            await _notifications.AddAsync(entity);
            await _notifications.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
            Guid userId,
            bool onlyUnread,
            CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var query = db.Notifications
                .Where(n => n.UserId == userId);

            if (onlyUnread)
                query = query.Where(n => !n.IsRead);

            // Якщо є CreatedAt у BaseEntity – краще сортувати по ньому
            query = query
                .OrderBy(n => n.IsRead)          // спочатку непрочитані
                .ThenByDescending(n => n.Id);    // приблизно новіші зверху

            var list = await query
                .Take(50) // щоб не вивалити тисячі нотифікацій за раз
                .ToListAsync(ct);

            return list
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Type,
                    n.PayloadJson,
                    n.IsRead,
                    (n as BaseEntity)?.CreatedAt ?? default   // якщо CreatedAt нема – заміни як треба
                ))
                .ToList();
        }

        public async Task MarkAsReadAsync(
            Guid notificationId,
            Guid userId,
            CancellationToken ct = default)
        {
            var entity = await _notifications.GetByKeyAsync(notificationId);

            if (entity == null || entity.UserId != userId)
                return; // тихо ігноруємо чужі/несуществуючі

            if (!entity.IsRead)
            {
                entity.IsRead = true;
                await _notifications.UpdateAsync(entity);
                await _notifications.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var list = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(ct);

            if (list.Count == 0)
                return;

            foreach (var n in list)
                n.IsRead = true;

            await db.SaveChangesAsync(ct);
        }
    }
}
