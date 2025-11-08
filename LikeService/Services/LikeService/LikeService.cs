using LibraryShared.Services.FeedClientService;
using LibraryShared.Services.RabbitMqPublisher;
using LibraryShared.Services.UserClientService;
using LikeService.Data;
using LikeService.Dtos;
using LikeService.Models;
using Microsoft.EntityFrameworkCore;

namespace LikeService.Services.LikeService
{
    public class LikeService : ILikeService
    {
        private readonly LikeContext _context;
        private readonly IUserClientService _userClient;
        private readonly IFeedClientService _feedClient;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public LikeService(LikeContext context, 
            IUserClientService userClient,
            IFeedClientService feedClient,
            IRabbitMqPublisher rabbitMqPublisher

            )
        {
            _context = context;
            _userClient = userClient;
            _feedClient = feedClient;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<bool> AddLikeAsync(LikeDto dto)
        {
            if (!await _userClient.CheckUserExistsAsync(dto.UserId))
                throw new Exception("Invalid user ID");

            if (!await _feedClient.CheckFeedPostExistsAsync(dto.PostId))
                throw new Exception("Invalid post ID");

            var post = await _feedClient.GetPostByIdAsync(dto.PostId);
            if (post == null)
                throw new Exception("Invalid post ID");
            if (await _context.Likes.AnyAsync(l => l.UserId == dto.UserId && l.PostId == dto.PostId))
                return false;

            var like = new Like { UserId = dto.UserId, PostId = dto.PostId };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                Event = "PostLiked",
                PostId = dto.PostId,
                LikerId = dto.UserId,
                OwnerId = post.UserId
            }, "notification_queue");


            return true;
        }

        public async Task<bool> RemoveLikeAsync(LikeDto dto)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == dto.UserId && l.PostId == dto.PostId);

            if (like == null) return false;

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetLikesCountAsync(int postId)
        {
            return await _context.Likes.CountAsync(l => l.PostId == postId);
        }
    }
}

