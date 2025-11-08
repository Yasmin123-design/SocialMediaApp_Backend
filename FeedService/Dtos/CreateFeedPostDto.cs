namespace FeedService.Dtos
{
    public class CreateFeedPostDto
    {
        public int? imageId { get; set; }
        public string? Content { get; set; }
        public string? PostType { get; set; } = "text";
        public IFormFile? MediaFile { get; set; }
    }
}
