using FollowService.Data;
using FollowService.Dtos;
using FollowService.Models;
using LibraryShared.Services.RabbitMqPublisher;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;

namespace FollowService.Services.FollowService
{
    public class FollowService : IFollowService
    {
        private readonly FollowContext _context;
        private readonly IUserClientService _userClientService;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public FollowService(FollowContext context,
            IUserClientService userClientService,
            IRabbitMqPublisher rabbitMqPublisher
            )
        {
            _context = context;
            _userClientService = userClientService;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<bool> FollowAsync(FollowDto dto)
        {
            if (dto.FollowerId == dto.FollowingId)
                return false;

            bool userExists = await _userClientService.CheckUserExistsAsync(dto.FollowerId);
            bool targetExists = await _userClientService.CheckUserExistsAsync(dto.FollowingId);
            if (!userExists || !targetExists) return false;

            var existing = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowingId == dto.FollowingId);
            if (existing != null) return false;

            _context.Follows.Add(new Follow
            {
                FollowerId = dto.FollowerId,
                FollowingId = dto.FollowingId
            });
            await _context.SaveChangesAsync();

            var message = new
            {
                FollowerId = dto.FollowerId,
                FollowingId = dto.FollowingId,
                Event = "Follow"
            };

            await _rabbitMqPublisher.PublishAsync(message, "notification_queue");
            return true;
        }

        public async Task<bool> UnfollowAsync(FollowDto dto)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowingId == dto.FollowingId);

            if (follow == null) return false;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            var message = new
            {
                FollowerId = dto.FollowerId,
                FollowingId = dto.FollowingId,
                Action = "UnFollow"
            };

            await _rabbitMqPublisher.PublishAsync(message, "notification_queue");
            return true;
        }

        public async Task<int> GetFollowersCountAsync(string userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowerId == userId);
        }
    }
}
