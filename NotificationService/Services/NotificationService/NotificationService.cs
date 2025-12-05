using LibraryShared.Services.UserClientService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Dtos;
using NotificationService.Helpers;
using NotificationService.Hubs;
using NotificationService.Models;

namespace NotificationService.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationContext _context;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IUserClientService _userClient;

        public NotificationService(
            NotificationContext context,
            IHubContext<NotificationHub> hub,
            IUserClientService userClientService
            )
        {
            _context = context;
            _hub = hub;
            _userClient = userClientService;
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            if (!await _userClient.CheckUserExistsAsync(userId))
                throw new Exception("Invalid user ID");

            var notification = new Notification
            {
                UserId = userId,
                Message = message
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hub.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(string userId)
        {
            var notifications = await _context.Notifications
      .Where(n => n.UserId == userId)
      .OrderByDescending(n => n.CreatedAt)
      .ToListAsync();

            var user = await _userClient.GetUserByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found in User Microservice.");

            return notifications.Select(n => n.ToDto(user));
        }
        public async Task MarkAllAsReadAsync(string userId)
        {
            var userNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!userNotifications.Any())
                return;

            foreach (var notification in userNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                throw new Exception("Notification not found");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
