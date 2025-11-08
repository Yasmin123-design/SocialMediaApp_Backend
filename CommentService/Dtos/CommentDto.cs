namespace CommentService.Dtos
{
    public class CommentDto
    {
        public int PostId { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
    }
}
