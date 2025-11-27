using LibraryShared.Dtos;
using UserService.Dtos;

namespace UserService.Services.ProfileService
{
    public interface IProfileService
    {
        Task<UserDto?> UpdateProfileImageAsync(string userId, string imagePath);
        Task<UserDto?> GetCurrentUserAsync(string userId);
        Task<UserDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<ChangePasswordResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
