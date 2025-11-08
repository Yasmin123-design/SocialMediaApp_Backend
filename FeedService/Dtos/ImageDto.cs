namespace FeedService.Dtos
{
    public class ImageDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string OriginalFilePath { get; set; } = null!;
        public string? FilteredFilePath { get; set; }
        public string? FilterApplied { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
