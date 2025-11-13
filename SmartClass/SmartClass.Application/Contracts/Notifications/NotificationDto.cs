namespace SmartClass.Application.Contracts.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
