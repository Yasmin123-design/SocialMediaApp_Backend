namespace ChatService.Dtos
{
    public class SendMessageRequestDto
    {
        public int ThreadId { get; set; }
        public string? SenderId { get; set; } = null!;
        public string Text { get; set; } = null!;
    }
}
