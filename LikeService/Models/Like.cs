namespace LikeService.Models
{
     public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
