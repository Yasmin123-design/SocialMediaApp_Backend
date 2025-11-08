using System.ComponentModel.DataAnnotations;

namespace ImageService.Dtos
{
    public class UploadImageDto
    {
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Image file is required.")]
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }
    }
}
