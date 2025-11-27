using ImageService.Dtos;
using ImageService.Services.ImageService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ImageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetImageById(int id)
        {
            var image = await _imageService.GetImageByIdAsync(id);

            if (image == null)
                return NotFound(new { message = "Image not found" });

            return Ok(image);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadImageDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var id = await _imageService.SaveImageAsync(dto.File, userId ,dto.Content);
            return Ok(new { imageId = id });
        }


        [HttpPost("apply-filter")]
        public async Task<IActionResult> ApplyFilter([FromBody] ApplyFilterDto dto)
        {
            var (bytes, filterName , filterPath) = await _imageService.ApplyFilterAsync(dto.ImageId, dto.FilterName, dto.Intensity);
            return Ok(new { FiteredNameApplied = filterName , FilteredPath = filterPath });
        }
    }
}

