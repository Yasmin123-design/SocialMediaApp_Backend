using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ChatService.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int ThreadId { get; set; }
        [ForeignKey(nameof(ThreadId))]
        public MessageThread Thread { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string SenderId { get; set; } = null!;

        [Required]
        public string Text { get; set; } = null!;

        public string? Metadata { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? EditedAt { get; set; }
    }
}
