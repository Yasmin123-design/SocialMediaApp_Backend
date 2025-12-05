namespace ChatService.Dtos
{
    public class StartChatRequestDto
    {
        public string? UserA { get; set; } = null!; // caller
        public string UserB { get; set; } = null!; // target
        public string? Title { get; set; }
    }
}
