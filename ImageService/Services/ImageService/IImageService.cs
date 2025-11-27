
using ImageService.Models;

namespace ImageService.Services.ImageService
{
    public interface IImageService
    {
        Task<Image?> GetImageByIdAsync(int imageId);

        Task<string> SaveImageAsync(IFormFile file, string userId,string? content, CancellationToken ct = default);
        Task<(byte[] ImageBytes, string FilteredFileName, string FilteredFilePath)> ApplyFilterAsync(string imageId, string filterName, int? intensity = null, CancellationToken ct = default);
    }
}
