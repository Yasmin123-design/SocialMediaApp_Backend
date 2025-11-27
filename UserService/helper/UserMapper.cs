using LibraryShared.Dtos;
using UserService.Models;

namespace UserService.helper
{
    public static class UserMapper
    {
        public static UserDto UserToDto(ApplicationUser user, string? message = null, string? token = null)
        {
            if (user == null) return null!;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Age = user.Age,
                Position = user.Position,
                Gender = user.Gender.HasValue ? (LibraryShared.Enums.Gender?)user.Gender.Value : null,
                MaritalStatus = user.MaritalStatus.HasValue ? (LibraryShared.Enums.MaritalStatus?)user.MaritalStatus.Value : null,
                Image = user.Image,
                Message = message,
                Token = token
            };
        }
    }
}

