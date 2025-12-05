using FeedService.Data;
using FeedService.Dtos;
using FeedService.helpers;
using FeedService.Models;
using FeedService.Services.ImageClientService;
using LibraryShared.Dtos;
using LibraryShared.Services.RabbitMqPublisher;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SearchService.Models;

namespace FeedService.Services.FeedService
{
    public class FeedService : IFeedService
    {
        private readonly FeedDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IUserClientService _userClientService;
        private readonly IImageClientService _imageClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public FeedService(
            FeedDbContext context,
            IWebHostEnvironment env,
            IUserClientService userClientService,
            IImageClientService imageClientService,
            IHttpContextAccessor httpContextAccessor,
            IRabbitMqPublisher rabbitMqPublisher
        )
        {
            _context = context;
            _env = env;
            _userClientService = userClientService;
            _imageClient = imageClientService;
            _httpContextAccessor = httpContextAccessor;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        private async Task<UserDto?> GetUserAsync(string userId)
        {
            if (!await _userClientService.CheckUserExistsAsync(userId))
                return null;

            return await _userClientService.GetUserByIdAsync(userId);
        }

        private async Task<ImageDto?> GetImageForPostAsync(int? imageId)
        {
            if (!imageId.HasValue)
                return null;

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            return await _imageClient.GetImageByIdAsync(imageId.Value, token);
        }

        private async Task<bool> ValidateUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await _userClientService.CheckUserExistsAsync(userId);
        }

        public async Task<IEnumerable<FeedPostWithUserDto>> GetUserFeedAsync(string userId)
        {
            if (!await ValidateUserAsync(userId))
                throw new KeyNotFoundException("User not found");

            var posts = await _context.FeedPosts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var user = await GetUserAsync(userId);

            var result = new List<FeedPostWithUserDto>();

            foreach (var post in posts)
            {
                var image = await GetImageForPostAsync(post.ImageId);
                result.Add(FeedPostMapper.ToFeedDto(post, user, image));
            }

            return result;
        }

        public async Task<FeedPostWithUserDto> GetByIdAsync(int id)
        {
            var post = await _context.FeedPosts.FindAsync(id)
                ?? throw new KeyNotFoundException("Post not found");

            var user = await GetUserAsync(post.UserId!);
            var image = await GetImageForPostAsync(post.ImageId);

            return FeedPostMapper.ToFeedDto(post, user, image);
        }

        public async Task<IEnumerable<FeedPostWithUserDto>> GetAllAsync()
        {
            var posts = await _context.FeedPosts
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<FeedPostWithUserDto>();

            foreach (var post in posts)
            {
                var user = await GetUserAsync(post.UserId!);
                var image = await GetImageForPostAsync(post.ImageId);

                result.Add(FeedPostMapper.ToFeedDto(post, user, image));
            }

            return result;
        }

        public async Task<FeedPostWithUserDto> CreateAsync(CreateFeedPostDto dto, string userId)
        {
            if (!await ValidateUserAsync(userId))
                throw new KeyNotFoundException("User not found");

            string? filePath = null;

            if (dto.MediaFile != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.MediaFile.FileName)}";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await dto.MediaFile.CopyToAsync(stream);

                filePath = $"/uploads/{fileName}";
            }

            var post = new FeedPost
            {
                UserId = userId,
                ImageId = dto.imageId,
                Content = dto.Content,
                PostType = dto.PostType ?? "text",
                MediaUrl = filePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.FeedPosts.Add(post);
            await _context.SaveChangesAsync();

            var user = await GetUserAsync(userId);
            var image = await GetImageForPostAsync(post.ImageId);

            await _rabbitMqPublisher.PublishAsync(new SearchIndexDocument
            {
                Id = post.Id.ToString(),
                Type = "post",
                Title = post.PostType,
                Content = post.Content
            }, "search_index_queue");

            return FeedPostMapper.ToFeedDto(post, user, image);
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _context.FeedPosts.FirstOrDefaultAsync(x => x.Id == id);
            if (post == null) return;

            if (post.ImageId != null)
                await _imageClient.DeleteImageAsync(post.ImageId.Value);

            _context.FeedPosts.Remove(post);
            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                Id = post.Id.ToString(),
                Action = "Delete"
            }, "search_index_queue");
        }

        public async Task<int> GetUserPostsCountAsync(string userId)
        {
            if (!await ValidateUserAsync(userId))
                throw new KeyNotFoundException("User not found");

            return await _context.FeedPosts.CountAsync(p => p.UserId == userId);
        }

        public async Task<(bool success, string? error)> SavePostAsync(string userId, int postId)
        {
            if (!await ValidateUserAsync(userId))
                return (false, "User not found");

            if (!await _context.FeedPosts.AnyAsync(p => p.Id == postId))
                return (false, "Post not found");

            var exists = await _context.SavedPosts
                .AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);

            if (exists)
                return (false, "Post already saved");

            _context.SavedPosts.Add(new SavedPost
            {
                UserId = userId,
                PostId = postId,
                SavedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }


        public async Task<IEnumerable<FeedPostWithUserDto>> GetSavedPostsAsync(string userId)
        {
            if (!await ValidateUserAsync(userId))
                throw new KeyNotFoundException("User not found");

            var savedPosts = await _context.SavedPosts
                .Where(sp => sp.UserId == userId)
                .Include(sp => sp.Post)
                .ToListAsync();

            var result = new List<FeedPostWithUserDto>();

            foreach (var sp in savedPosts)
            {
                var user = await GetUserAsync(sp.Post.UserId!);
                var image = await GetImageForPostAsync(sp.Post.ImageId);

                result.Add(FeedPostMapper.ToFeedDto(sp.Post, user, image));
            }

            return result;
        }
        public async Task<(bool success, string? error)> RemoveSavedPostAsync(string userId, int postId)
        {
            if (!await ValidateUserAsync(userId))
                return (false, "User not found");

            var savedPost = await _context.SavedPosts
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

            if (savedPost == null)
                return (false, "Post is not in saved list");

            _context.SavedPosts.Remove(savedPost);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool success, string error)> SharePostAsync(string userId, int postId)
        {
            var post = await _context.FeedPosts.FindAsync(postId);
            if (post == null)
                return (false, "Post not found");

            var sharedPost = new FeedPost
            {
                Content = post.Content,
                PostType = post.PostType,
                MediaUrl = post.MediaUrl,
                ImageId = post.ImageId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.FeedPosts.Add(sharedPost);
            await _context.SaveChangesAsync();

            return (true, null);
        }

    }
}

    

