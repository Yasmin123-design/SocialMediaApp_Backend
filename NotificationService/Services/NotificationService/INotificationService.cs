using NotificationService.Models;

namespace NotificationService.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task<IEnumerable<Notification>> GetNotificationsAsync(string userId);
    }
}
