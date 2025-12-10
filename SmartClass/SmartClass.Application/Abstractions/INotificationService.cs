using SmartClass.Application.Contracts.Notifications;

namespace SmartClass.Application.Abstractions
{
    public interface INotificationService
    {
        /// <summary>
        /// Створити нове сповіщення для користувача.
        /// Викликається іншими сервісами (оцінка, нове завдання тощо).
        /// </summary>
        Task CreateAsync(CreateNotificationDto dto, CancellationToken ct = default);

        /// <summary>
        /// Отримати список сповіщень для користувача.
        /// </summary>
        Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
            Guid userId,
            bool onlyUnread,
            CancellationToken ct = default);

        /// <summary>
        /// Позначити одне сповіщення як прочитане.
        /// </summary>
        Task MarkAsReadAsync(
            Guid notificationId,
            Guid userId,
            CancellationToken ct = default);

        /// <summary>
        /// Позначити всі сповіщення користувача як прочитані.
        /// </summary>
        Task MarkAllAsReadAsync(
            Guid userId,
            CancellationToken ct = default);
    }
}
