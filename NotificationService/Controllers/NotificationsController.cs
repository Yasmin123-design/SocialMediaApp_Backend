using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services.NotificationService;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] Notification request)
        {
            await _notificationService.SendNotificationAsync(request.UserId, request.Message);
            return Ok(new { Message = "Notification sent successfully!" });
        }

        [HttpGet("usernotifications")]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token" });

            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpPut("mark-read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                await _notificationService.MarkAsReadAsync(id, userId);
                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
