using System.ComponentModel.DataAnnotations;

namespace ChatService.Models
{
    public class MessageThread
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(250)]
        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastMessageAt { get; set; }

        public ICollection<MessageThreadParticipant> Participants { get; set; } = new List<MessageThreadParticipant>();

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}

