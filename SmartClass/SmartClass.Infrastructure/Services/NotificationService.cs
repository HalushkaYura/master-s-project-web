using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;
using System.Text.Json;

namespace SmartClass.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext db;

    public NotificationService(AppDbContext db) => this.db = db;

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
    }

    // (пізніше можемо додати fan-out у SignalR тут)
}
