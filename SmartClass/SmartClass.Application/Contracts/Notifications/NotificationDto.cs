namespace SmartClass.Application.Contracts.Notifications
{

    /// <summary>
    /// Те, що віддаємо на фронт.
    /// </summary>
    public sealed record NotificationDto(
        Guid Id,
        string Type,
        string PayloadJson,
        bool IsRead,
        DateTime CreatedAt  
    );

}