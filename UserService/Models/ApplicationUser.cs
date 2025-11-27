using Microsoft.AspNetCore.Identity;

namespace UserService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? Age { get; set; }
        public string? Address { get; set; }
        public string? Image { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        public Gender? Gender { get; set; }
        public string? Position { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
