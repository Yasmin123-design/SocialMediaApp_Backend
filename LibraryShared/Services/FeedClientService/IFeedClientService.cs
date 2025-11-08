

using LibraryShared.Dtos;

namespace LibraryShared.Services.FeedClientService
{
    public interface IFeedClientService
    {
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<bool> CheckFeedPostExistsAsync(int postId);
        Task<bool> CreatePostFromImageAsync(string userId, int imageId, string mediaUrl, CancellationToken ct = default);

    }
}
