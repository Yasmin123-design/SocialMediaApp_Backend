using FollowService.Dtos;
using LibraryShared.Dtos;

namespace FollowService.Services.FollowService
{
    public interface IFollowService
    {
        Task<bool> FollowAsync(FollowDto dto);
        Task<bool> UnfollowAsync(FollowDto dto);
        Task<int> GetFollowersCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
        Task<List<UserDto>> GetFollowingUsersAsync(string userId);
        Task<List<UserDto>> GetFollowersUsersAsync(string userId);

    }
}
