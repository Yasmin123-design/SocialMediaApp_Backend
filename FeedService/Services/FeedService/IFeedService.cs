using FeedService.Dtos;
using FeedService.Models;
using LibraryShared.Dtos;

namespace FeedService.Services.FeedService
{
    public interface IFeedService
    {
        Task<int> GetUserPostsCountAsync(string userId);
        Task<IEnumerable<FeedPostWithUserDto>> GetUserFeedAsync(string userId);
        Task<IEnumerable<FeedPostWithUserDto>> GetAllAsync();
        Task<FeedPostWithUserDto> GetByIdAsync(int id);
        Task<FeedPostWithUserDto> CreateAsync(CreateFeedPostDto dto, string userId);
        Task DeleteAsync(int id);
        Task<(bool success, string? error)> SavePostAsync(string userId, int postId);
        Task<IEnumerable<FeedPostWithUserDto>> GetSavedPostsAsync(string userId);
        Task<(bool success, string? error)> RemoveSavedPostAsync(string userId, int postId);

    }
}
