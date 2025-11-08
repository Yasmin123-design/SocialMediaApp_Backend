using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Services.ProfileService
{
    public class ProfileService : IProfileService
    {
        private readonly UserDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ProfileService(UserDbContext context , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<UserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.FullName,
                Email = user.Email
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = dto.Username ?? user.FullName;
            user.Email = dto.Email ?? user.Email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return false;

            await _context.SaveChangesAsync();
            return true;
        }


        private string ComputeHash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
