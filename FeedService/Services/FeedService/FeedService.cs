using FeedService.Data;
using FeedService.Dtos;
using FeedService.Models;
using FeedService.Services.ImageClientService;
using LibraryShared.Services.RabbitMqPublisher;
using LibraryShared.Services.UserClientService;
using Microsoft.EntityFrameworkCore;
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
        public FeedService(FeedDbContext context,
            IWebHostEnvironment env ,
            IUserClientService userClientService,
            IImageClientService imageClientService,
            IHttpContextAccessor httpContextAccessor,
            IRabbitMqPublisher rabbitMqPublisher
            )
        {
            _imageClient = imageClientService;
            _rabbitMqPublisher = rabbitMqPublisher;
            _context = context;
            _userClientService = userClientService;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<FeedPostWithUserDto>> GetUserFeedAsync(string userId)
        {
            var posts = await _context.FeedPosts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var user = await _userClientService.GetUserByIdAsync(userId);

            var result = new List<FeedPostWithUserDto>();

            foreach (var post in posts)
            {
                string? originalImagePath = null;
                string? filteredImagePath = null;

                if (post.ImageId != null)
                {
                    var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    var image = await _imageClient.GetImageByIdAsync(post.ImageId.Value,token);
                    if (image != null)
                    {
                        originalImagePath = image.OriginalFilePath;
                        filteredImagePath = image.FilteredFilePath;
                    }
                }

                result.Add(new FeedPostWithUserDto
                {
                    Id = post.Id,
                    ImageId = post.ImageId,
                    Content = post.Content,
                    MediaUrl = post.MediaUrl,
                    PostType = post.PostType,
                    CreatedAt = post.CreatedAt,
                    UserId = user?.Id,
                    UserName = user?.FullName,
                    UserEmail = user?.Email,
                    OriginalImagePath = originalImagePath,
                    FilteredImagePath = filteredImagePath
                });
            }

            return result;
        }

        public async Task<FeedPostWithUserDto> GetByIdAsync(int id)
        {
            var post = await _context.FeedPosts.FindAsync(id)
                ?? throw new KeyNotFoundException("Post not found");

            var user = await _userClientService.GetUserByIdAsync(post.UserId!);

            ImageDto? image = null;
            if (post.ImageId.HasValue)
            {
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                image = await _imageClient.GetImageByIdAsync(post.ImageId.Value, token);
            }

            return new FeedPostWithUserDto
            {
                Id = post.Id,
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                PostType = post.PostType,
                CreatedAt = post.CreatedAt,
                UserId = user?.Id,
                UserName = user?.FullName,
                UserEmail = user?.Email,
                ImageId = post.ImageId,
                OriginalImagePath = image?.OriginalFilePath,
                FilteredImagePath = image?.FilteredFilePath
            };
        }
        public async Task<IEnumerable<FeedPostWithUserDto>> GetAllAsync()
        {
            var posts = await _context.FeedPosts
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<FeedPostWithUserDto>();

            foreach (var post in posts)
            {
                var user = await _userClientService.GetUserByIdAsync(post.UserId!);

                ImageDto? image = null;
                if (post.ImageId.HasValue)
                {
                    var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    image = await _imageClient.GetImageByIdAsync(post.ImageId.Value, token);
                }

                result.Add(new FeedPostWithUserDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    MediaUrl = post.MediaUrl,
                    PostType = post.PostType,
                    CreatedAt = post.CreatedAt,
                    UserId = user?.Id,
                    UserName = user?.FullName,
                    UserEmail = user?.Email,
                    ImageId = post.ImageId,
                    OriginalImagePath = image?.OriginalFilePath,
                    FilteredImagePath = image?.FilteredFilePath
                });
            }

            return result;
        }


        public async Task<FeedPost> CreateAsync(CreateFeedPostDto dto, string userId)
        {
            string? filePath = null;

            if (dto.MediaFile != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.MediaFile.FileName)}";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.MediaFile.CopyToAsync(stream);
                }

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

            var searchDoc = new SearchIndexDocument
            {
                Id = post.Id.ToString(),
                Type = "post",
                Title = post.PostType,
                Content = post.Content
            };

            await _rabbitMqPublisher.PublishAsync(searchDoc, "search_index_queue");

            return post;
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _context.FeedPosts.FirstOrDefaultAsync(x => x.Id == id);
            if (post == null) return;

            if (post.ImageId != null)
            {
                await _imageClient.DeleteImageAsync(post.ImageId.Value);
            }
            _context.FeedPosts.Remove(post);
            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(new
            {
                Id = post.Id.ToString(),
                Action = "Delete"
            }, "search_index_queue");
        }
    }
}
    

