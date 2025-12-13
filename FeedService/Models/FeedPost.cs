namespace FeedService.Models
{
    public class FeedPost
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public int? ImageId { get; set; }
        public string PostType { get; set; } = "text"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } 

    }
}


