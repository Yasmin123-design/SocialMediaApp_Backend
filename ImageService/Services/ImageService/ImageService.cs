using ImageService.Data;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using Img = SixLabors.ImageSharp.Image;
using Image = ImageService.Models.Image;
using LibraryShared.Services.UserClientService;
using LibraryShared.Services.FeedClientService;
using LibraryShared.Services.RabbitMqPublisher;
using SearchService.Models;

namespace ImageService.Services.ImageService
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ImageDbContext _context;
        private readonly IUserClientService _userClientService;
        private readonly string _uploadsFolder;
        private readonly IFeedClientService _feedClientService;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        public ImageService(
            IWebHostEnvironment env,
            ImageDbContext context,
            IUserClientService userClientService,
            IFeedClientService feedClientService,
            IRabbitMqPublisher rabbitMqPublisher
            )
        {
            _env = env;
            _feedClientService = feedClientService;
            _rabbitMqPublisher = rabbitMqPublisher;
            _context = context;
            _userClientService = userClientService;
            _uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(_uploadsFolder);
        }

        public async Task<string> SaveImageAsync(IFormFile file, string userId, CancellationToken ct = default)
        {
            if (!await _userClientService.CheckUserExistsAsync(userId))
                throw new ArgumentException("Invalid UserId. User does not exist.");

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExt.Contains(ext))
                throw new ArgumentException("Invalid file type.");

            if (file.Length > 20 * 1024 * 1024)
                throw new ArgumentException("File too large (Max 20MB).");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var relativePath = $"/uploads/{fileName}";
            var fullPath = Path.Combine(_uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            var image = new Image
            {
                UserId = userId,
                FileName = fileName,
                OriginalFilePath = relativePath,
                UploadedAt = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync(ct);

            var success = await _feedClientService.CreatePostFromImageAsync(userId, image.Id, relativePath, ct);
            if (!success)
                throw new Exception("Failed to create feed post for uploaded image.");

            var searchDoc = new SearchIndexDocument
            {
                Id = image.Id.ToString(),
                Type = "image",
                Title = "Image",
                Content = image.FilterApplied ?? ""
            };

            await _rabbitMqPublisher.PublishAsync(searchDoc, "search_index_queue");

            return image.Id.ToString();
        }

        public async Task<Image?> GetImageByIdAsync(int imageId)
        {
            var image = await _context.Images
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                return null;
            return image;
        }
            public async Task<(byte[] ImageBytes, string FilteredFileName, string FilteredFilePath)> ApplyFilterAsync(
            string imageId,
            string filterName,
            int? intensity = null,
            CancellationToken ct = default)
        {
            var imageEntity = await _context.Images.FirstOrDefaultAsync(i => i.Id.ToString() == imageId, ct);
            if (imageEntity == null)
                throw new FileNotFoundException("Image not found in database.");

            var originalPath = imageEntity.OriginalFilePath;
            var fullOriginalPath = Path.Combine(_env.WebRootPath ?? "wwwroot", originalPath.TrimStart('/'));

            if (!File.Exists(fullOriginalPath))
                throw new FileNotFoundException("Original image file not found at: " + fullOriginalPath);

            using (var fs = File.OpenRead(fullOriginalPath))
            using (var image = await Img.LoadAsync(fs, ct))
            {
                switch (filterName?.ToLowerInvariant())
                {
                    case "grayscale":
                        image.Mutate(x => x.Grayscale());
                        break;

                    case "sepia":
                        image.Mutate(x => x.Sepia());
                        break;

                    case "invert":
                        image.Mutate(x => x.Invert());
                        break;

                    case "blur":
                        var sigma = Math.Max(0, (int)(intensity ?? 5));
                        image.Mutate(x => x.GaussianBlur(sigma));
                        break;

                    case "brightness":
                        var amount = 1f + ((intensity ?? 0) / 100f);
                        image.Mutate(x => x.Brightness(amount));
                        break;

                    default:
                        throw new ArgumentException("Unknown filter name.");
                }

                var filteredName = $"{Path.GetFileNameWithoutExtension(imageEntity.FileName)}_{filterName}.png";
                var filteredFullPath = Path.Combine(_uploadsFolder, filteredName);
                var relativeFilteredPath = $"/uploads/{filteredName}";

                if (File.Exists(filteredFullPath))
                    File.Delete(filteredFullPath);

                using (var outStream = File.OpenWrite(filteredFullPath))
                {
                    await image.SaveAsync(outStream, new PngEncoder(), ct);
                }

                imageEntity.FilterApplied = filterName;
                imageEntity.FilteredFilePath = relativeFilteredPath;

                await _context.SaveChangesAsync(ct);

                var imageBytes = await File.ReadAllBytesAsync(filteredFullPath, ct);
                return (imageBytes, filteredName, relativeFilteredPath);
            }
        }
    }
}


