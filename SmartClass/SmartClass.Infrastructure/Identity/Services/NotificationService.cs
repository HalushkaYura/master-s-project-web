using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Notifications;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;
using System.Text.Json;

namespace SmartClass.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext db;
    private readonly INotificationRealtimeSender realtimeSender;

    public NotificationService(AppDbContext db, INotificationRealtimeSender realtimeSender)
    {
        this.db = db;
        this.realtimeSender = realtimeSender;
    }

    public async Task CreateAsync(Guid userId, string type, string payloadJson, CancellationToken ct = default)
    {
        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await db.Notifications.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);

        // DTO для пуша
        var dto = new NotificationDto
        {
            Id = entity.Id,
            Type = entity.Type,
            PayloadJson = entity.PayloadJson,
            IsRead = entity.IsRead,
            CreatedAt = entity.CreatedAt
        };

        await realtimeSender.SendToUserAsync(userId, dto, ct);
    }
}
