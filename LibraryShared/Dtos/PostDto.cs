

namespace LibraryShared.Dtos
{
    public class PostDto
    {

        public string UserId { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public string? PostType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
