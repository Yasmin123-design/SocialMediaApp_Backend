using NotificationService.Dtos;
using NotificationService.Models;

namespace NotificationService.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(string userId);
        Task MarkAllAsReadAsync(string userId);
        Task MarkAsReadAsync(int id, string userId);
    }
}
