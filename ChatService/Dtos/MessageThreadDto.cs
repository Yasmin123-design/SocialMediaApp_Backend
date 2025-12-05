namespace ChatService.Dtos
{
    public class MessageThreadDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<string> ParticipantIds { get; set; } = new List<string>();
        public DateTime? LastMessageAt { get; set; }

    }
}
