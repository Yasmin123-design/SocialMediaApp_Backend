using System.ComponentModel.DataAnnotations.Schema;

namespace FeedService.Models
{
    public class SavedPost
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!; 
        public int PostId { get; set; }  
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        public FeedPost Post { get; set; } = null!;
    }
}
