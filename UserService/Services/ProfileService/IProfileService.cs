using UserService.Dtos;

namespace UserService.Services.ProfileService
{
    public interface IProfileService
    {
        Task<UserDto> GetCurrentUserAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
