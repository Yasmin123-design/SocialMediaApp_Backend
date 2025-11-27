using LibraryShared.Dtos;
using LikeService.Dtos;

namespace LikeService.Services.LikeService
{
    public interface ILikeService
    {
        Task<IEnumerable<UserDto>> GetUsersWhoLikedPostAsync(int postId);
        Task<bool> CheckIfUserLikedPostAsync(int postId, string userId);
        Task<bool> AddLikeAsync(LikeDto dto);
        Task<bool> RemoveLikeAsync(LikeDto dto);
        Task<int> GetLikesCountAsync(int postId);
    }
}
