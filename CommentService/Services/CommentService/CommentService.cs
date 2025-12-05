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

        private async Task<CommentResult> MapToCommentResultAsync(Comment comment)
        {
            var user = await _userClientService.GetUserByIdAsync(comment.UserId);
            return new CommentResult
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                UserName = user?.FullName ?? "Unknown",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<IEnumerable<CommentResult>> GetCommentsByPostIdAsync(int postId)
        {
            bool postExists = await _feedClientService.CheckFeedPostExistsAsync(postId);
            if (!postExists)
                throw new Exception("Invalid post ID");

            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<CommentResult>();
            foreach (var comment in comments)
            {
                var commentResult = await MapToCommentResultAsync(comment);
                result.Add(commentResult);
            }

            return result;
        }

        public async Task<CommentResult?> AddCommentAsync(CommentDto dto)
        {
            bool userExists = await _userClientService.CheckUserExistsAsync(dto.UserId);
            bool postExists = await _feedClientService.CheckFeedPostExistsAsync(dto.PostId);

            if (!userExists || !postExists)
                throw new Exception("Invalid user or post ID");

            var user = await _userClientService.GetUserByIdAsync(dto.UserId);
            if (user == null)
                return null;

            var comment = new Comment
            {
                PostId = dto.PostId,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(); 

            var commentResult = await MapToCommentResultAsync(comment);

            var post = await _feedClientService.GetPostByIdAsync(dto.PostId);
            if (!string.IsNullOrEmpty(post.UserId))
            {
                await _rabbitMqPublisher.PublishAsync(new
                {
                    Event = "Comment",
                    PostId = dto.PostId,
                    CommenterId = dto.UserId,
                    OwnerId = post.UserId,
                    OwnerName = user.FullName
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

            return commentResult;
        }

        public async Task<CommentResult?> UpdateCommentAsync(int id, string userId, string newContent)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (comment == null) return null;

            comment.Content = newContent;
            comment.CreatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var commentResult = await MapToCommentResultAsync(comment);

            var searchDoc = new SearchIndexDocument
            {
                Id = comment.Id.ToString(),
                Type = "comment",
                Title = "Comment",
                Content = comment.Content
            };
            await _rabbitMqPublisher.PublishAsync(searchDoc, "search_index_queue");

            return commentResult;
        }

        public async Task<int> GetNoOfCommentsOfPostAsync(int postId)
        {
            return await _context.Comments
                                 .Where(c => c.PostId == postId)
                                 .CountAsync();
        }
        public async Task<CommentResult?> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (comment == null) return null;

            var post = await _feedClientService.GetPostByIdAsync(comment.PostId);
            if (post == null) return null;

            var user = await _userClientService.GetUserByIdAsync(post.UserId);
            if (user == null) return null;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                Event = "UnComment",
                PostId = comment.PostId,
                CommenterId = userId,
                OwnerId = post.UserId,
                OwnerName = user.FullName
            }, "notification_queue");

            await _rabbitMqPublisher.PublishAsync(new
            {
                Id = comment.Id.ToString(),
                Action = "Delete"
            }, "search_index_queue");

            var commentResult = await MapToCommentResultAsync(comment);
            return commentResult;
        }
    }
}
