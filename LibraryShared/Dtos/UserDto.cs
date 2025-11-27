

using LibraryShared.Enums;

namespace LibraryShared.Dtos
{
    public class UserDto
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public string? Position { get; set; }
        public Gender? Gender { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        public string? Image { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
    }
}
