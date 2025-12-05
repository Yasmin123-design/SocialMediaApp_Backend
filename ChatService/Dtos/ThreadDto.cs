namespace ChatService.Dtos
{
    public class ThreadDto
    {
        public int ThreadId { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string LastMessage { get; set; }
         public string? ImageUser { get; set; }
        public DateTime LastMessageTime { get; set; }
    }
}
