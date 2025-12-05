using LibraryShared.Dtos;
using NotificationService.Dtos;
using NotificationService.Models;

namespace NotificationService.Helpers
{
    public static class NotificationSelector
    {
        public static NotificationResponseDto ToDto(this Notification n, UserDto user)
        {
            return new NotificationResponseDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,

                UserName = user.FullName,
                UserEmail = user.Email,
                UserImage = user.Image
            };
        }
    }
}
