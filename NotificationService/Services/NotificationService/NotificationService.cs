using LibraryShared.Services.UserClientService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
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

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
