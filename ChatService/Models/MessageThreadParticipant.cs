using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ChatService.Models
{
    public class MessageThreadParticipant
    {
        [Key]
        public int Id { get; set; }

        public int ThreadId { get; set; }
        [ForeignKey(nameof(ThreadId))]
        public MessageThread Thread { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public int UnreadCount { get; set; } = 0;
    }
}
