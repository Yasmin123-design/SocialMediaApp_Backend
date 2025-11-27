using LibraryShared.Dtos;

namespace FeedService.Dtos
{
    public class FeedPostWithUserDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public int? ImageId { get; set; }
        public string? OriginalImagePath { get; set; }
        public string? FilteredImagePath { get; set;}
        public string? PostType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserId { get; set; }
        public UserDto User { get; set; }
    }
}
