using LibraryShared.Dtos;
using UserService.Dtos;

namespace UserService.Services.AuthService
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto model);
        Task<UserDto> LoginAsync(LoginDto model);

        Task<UserDto?> GetUserByIdAsync(string id);
        Task<List<UserDto>> GetAllUsersAsync(); 


    }
}
