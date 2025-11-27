using LibraryShared.Dtos;
using LibraryShared.Services.UserClientService;
using Microsoft.AspNetCore.Identity;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Services.ProfileService
{

    public class ProfileService : IProfileService
    {
        private readonly UserDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserClientService _userClientService;

        public ProfileService(
            UserDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserClientService userClientService)
        {
            _context = context;
            _userManager = userManager;
            _userClientService = userClientService;
        }

        public async Task<UserDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrEmpty(dto.Email))
                await _userManager.SetEmailAsync(user, dto.Email);

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Address))
                user.Address = dto.Address;

            if (dto.Age.HasValue)
                user.Age = dto.Age;

            if (dto.Position != null)
                user.Position = dto.Position;

            if (dto.Gender.HasValue)
                user.Gender = dto.Gender;

            if (dto.MaritalStatus.HasValue)
                user.MaritalStatus = dto.MaritalStatus;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return await _userClientService.GetUserByIdAsync(userId);
        }

        public async Task<UserDto?> GetCurrentUserAsync(string userId)
        {
            return await _userClientService.GetUserByIdAsync(userId);
        }

        public async Task<ChangePasswordResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ChangePasswordResult
                {
                    Succeeded = false,
                    Errors = new[] { new IdentityError { Description = "User not found." } }
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                return new ChangePasswordResult
                {
                    Succeeded = false,
                    Errors = result.Errors
                };
            }

            var userDto = await _userClientService.GetUserByIdAsync(userId);

            return new ChangePasswordResult
            {
                Succeeded = true,
                UserDto = userDto
            };
        }

        public async Task<UserDto?> UpdateProfileImageAsync(string userId, string imagePath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.Image = imagePath;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return await _userClientService.GetUserByIdAsync(userId);
        }
    }


}
