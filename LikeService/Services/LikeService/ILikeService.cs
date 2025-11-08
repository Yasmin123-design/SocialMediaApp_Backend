using LikeService.Dtos;

namespace LikeService.Services.LikeService
{
    public interface ILikeService
    {
        Task<bool> AddLikeAsync(LikeDto dto);
        Task<bool> RemoveLikeAsync(LikeDto dto);
        Task<int> GetLikesCountAsync(int postId);
    }
}
