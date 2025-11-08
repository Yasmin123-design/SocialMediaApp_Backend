namespace ImageService.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string UserId { get; set; }  
        public string FileName { get; set; }
        public string? FilteredFilePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? FilterApplied { get; set; }
        public string OriginalFilePath { get; set; } = null!;
    }
}
