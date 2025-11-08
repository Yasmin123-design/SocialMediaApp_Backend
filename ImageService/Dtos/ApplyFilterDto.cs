using System.ComponentModel.DataAnnotations;

namespace ImageService.Dtos
{
    public class ApplyFilterDto
    {
        [Required(ErrorMessage = "ImageId is required.")]
        public string ImageId { get; set; }

        [Required(ErrorMessage = "FilterName is required.")]
        public string FilterName { get; set; }

        public int? Intensity { get; set; } 
    }
}
