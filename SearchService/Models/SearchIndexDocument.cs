namespace SearchService.Models
{
    public class SearchIndexDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } // post/comment/image
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
