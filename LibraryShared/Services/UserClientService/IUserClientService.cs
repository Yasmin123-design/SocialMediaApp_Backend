using LibraryShared.Dtos;

namespace LibraryShared.Services.UserClientService
{
    public interface IUserClientService
    {
        Task<bool> CheckUserExistsAsync(string userId);
        Task<UserDto?> GetUserByIdAsync(string userId);
    }
}
