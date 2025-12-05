namespace ChatService.Dtos
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderName { get; set; }
        public string? SenderImage { get; set; }
        public string Text { get; set; } = null!;
        public DateTime SentAt { get; set; }
    }
}
