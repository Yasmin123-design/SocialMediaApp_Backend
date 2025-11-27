using FollowService.Data;
using FollowService.Dtos;
using FollowService.Models;
using LibraryShared.Dtos;
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

        public FollowService(
            FollowContext context,
            IUserClientService userClientService,
            IRabbitMqPublisher rabbitMqPublisher)
        {
            _context = context;
            _userClientService = userClientService;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<bool> FollowAsync(FollowDto dto)
        {
            if (dto.FollowerId == dto.FollowingId) return false;

            if (!await _userClientService.CheckUserExistsAsync(dto.FollowerId) ||
                !await _userClientService.CheckUserExistsAsync(dto.FollowingId))
                return false;

            var existing = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowingId == dto.FollowingId);

            if (existing != null) return false;

            _context.Follows.Add(new Follow
            {
                FollowerId = dto.FollowerId,
                FollowingId = dto.FollowingId
            });

            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                dto.FollowerId,
                dto.FollowingId,
                Event = "Follow"
            }, "notification_queue");

            return true;
        }

        public async Task<bool> UnfollowAsync(FollowDto dto)
        {
            if (!await _userClientService.CheckUserExistsAsync(dto.FollowerId) ||
                !await _userClientService.CheckUserExistsAsync(dto.FollowingId))
                return false;

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowingId == dto.FollowingId);

            if (follow == null) return false;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                dto.FollowerId,
                dto.FollowingId,
                Event = "Unfollow"
            }, "notification_queue");

            return true;
        }

        private async Task<List<UserDto>> GetUsersAsync(List<string> ids)
        {
            var users = new List<UserDto>();

            foreach (var id in ids)
            {
                var user = await _userClientService.GetUserByIdAsync(id);
                if (user != null)
                    users.Add(user);
            }

            return users;
        }

        public async Task<List<UserDto>> GetFollowersUsersAsync(string userId)
        {
            if (!await _userClientService.CheckUserExistsAsync(userId))
                return new List<UserDto>();

            var followersIds = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => f.FollowerId)
                .ToListAsync();

            return await GetUsersAsync(followersIds);
        }

        public async Task<List<UserDto>> GetFollowingUsersAsync(string userId)
        {
            if (!await _userClientService.CheckUserExistsAsync(userId))
                return new List<UserDto>();

            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            return await GetUsersAsync(followingIds);
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

