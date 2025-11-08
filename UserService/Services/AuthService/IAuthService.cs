using UserService.Dtos;
using UserService.Models;

namespace UserService.Services.AuthService
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto model);
        Task<string> LoginAsync(LoginDto model);
        Task<ApplicationUser?> GetUserByIdAsync(string id);

    }
}
