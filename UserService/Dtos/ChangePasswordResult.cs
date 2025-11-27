using LibraryShared.Dtos;
using Microsoft.AspNetCore.Identity;

namespace UserService.Dtos
{
    public class ChangePasswordResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<IdentityError> Errors { get; set; } = Enumerable.Empty<IdentityError>();
        public UserDto? UserDto { get; set; }
    }
}
