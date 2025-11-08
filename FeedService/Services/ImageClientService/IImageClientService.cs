using FeedService.Dtos;

namespace FeedService.Services.ImageClientService
{
    public interface IImageClientService
    {
        Task<ImageDto?> GetImageByIdAsync(int imageId, string token);
        Task DeleteImageAsync(int imageId);

    }
}
