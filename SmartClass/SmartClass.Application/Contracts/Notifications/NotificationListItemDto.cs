namespace SmartClass.Application.Contracts.Notifications
{
    public sealed record NotificationListItemDto(
        Guid Id,
        Guid UserId,
        string Type,
        string PayloadJson,
        bool IsRead,
        DateTime CreatedAt
    );
}
