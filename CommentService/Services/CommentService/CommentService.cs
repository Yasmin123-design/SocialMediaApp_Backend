using CommentService.Data;
using CommentService.Dtos;
using CommentService.Models;
using LibraryShared.Services.FeedClientService;
using LibraryShared.Services.RabbitMqPublisher;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;
using SearchService.Models;

namespace CommentService.Services.CommentService
{
    public class CommentService : ICommentService
    {
        private readonly CommentContext _context;
        private readonly IUserClientService _userClientService;
        private readonly IFeedClientService _feedClientService;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public CommentService(CommentContext context,
            IUserClientService userClientService,
            IFeedClientService feedClientService,
            IRabbitMqPublisher rabbitMqPublisher
            )
        {
            _context = context;
            _userClientService = userClientService;
            _feedClientService = feedClientService;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            bool postExists = await _feedClientService.CheckFeedPostExistsAsync(postId);
            if (!postExists)
                throw new Exception("Invalid post ID");

            return await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> AddCommentAsync(CommentDto dto)
        {
            bool userExists = await _userClientService.CheckUserExistsAsync(dto.UserId);
            bool postExists = await _feedClientService.CheckFeedPostExistsAsync(dto.PostId);

            if (!userExists || !postExists)
                throw new Exception("Invalid user or post ID");

            var comment = new Comment
            {
                PostId = dto.PostId,
                UserId = dto.UserId,
                Content = dto.Content
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var post = await _feedClientService.GetPostByIdAsync(dto.PostId);
            if (!string.IsNullOrEmpty(post.UserId))
            {
                await _rabbitMqPublisher.PublishAsync(new
                {
                    Event = "Comment",
                    PostId = dto.PostId,
                    CommenterId = dto.UserId,
                    OwnerId = post.UserId
                }, "notification_queue");
            }

            var searchDoc = new SearchIndexDocument
            {
                Id = comment.Id.ToString(),
                Type = "comment",
                Title = "Comment",
                Content = comment.Content
            };

            await _rabbitMqPublisher.PublishAsync(searchDoc, "search_index_queue");
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                Id = comment.Id.ToString(),
                Action = "Delete"
            }, "search_index_queue");

            return true;
        }

        public async Task<Comment?> UpdateCommentAsync(int id, string userId, string newContent)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (comment == null) return null;

            comment.Content = newContent;
            await _context.SaveChangesAsync();

            var searchDoc = new SearchIndexDocument
            {
                Id = comment.Id.ToString(),
                Type = "comment",
                Title = "Comment",
                Content = comment.Content
            };
            await _rabbitMqPublisher.PublishAsync(searchDoc, "search_index_queue");

            return comment;
        }
    }
}
