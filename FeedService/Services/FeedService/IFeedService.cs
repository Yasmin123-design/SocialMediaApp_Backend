using FeedService.Dtos;
using FeedService.Models;

namespace FeedService.Services.FeedService
{
    public interface IFeedService
    {
        Task<IEnumerable<FeedPostWithUserDto>> GetUserFeedAsync(string userId);
        Task<IEnumerable<FeedPostWithUserDto>> GetAllAsync();
        Task<FeedPostWithUserDto> GetByIdAsync(int id);
        Task<FeedPost> CreateAsync(CreateFeedPostDto dto, string userId);
        Task DeleteAsync(int id);
    }
}
