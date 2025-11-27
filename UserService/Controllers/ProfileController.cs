using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Dtos;
using UserService.Services.FileStorageService;
using UserService.Services.ProfileService;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IFileStorageService _fileStorageService;

        public ProfileController(
            IProfileService profileService,
            IFileStorageService fileStorageService
            )
        {
            _profileService = profileService;
            _fileStorageService = fileStorageService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("me/{userId}")]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var userDto = await _profileService.GetCurrentUserAsync(userId);
            if (userDto == null)
                return NotFound(new { message = "User not found." });

            return Ok(userDto);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userDto = await _profileService.UpdateProfileAsync(GetUserId(), dto);
            if (userDto == null)
                return NotFound(new { message = "User not found." });

            return Ok(userDto);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = await _profileService.ChangePasswordAsync(GetUserId(), dto);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Password change failed.", errors });
            }

            return Ok(result.UserDto);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var imagePath = await _fileStorageService.SaveFileAsync(file, "profiles");

            var userDto = await _profileService.UpdateProfileImageAsync(GetUserId(), imagePath);
            if (userDto == null)
                return NotFound(new { message = "User not found." });

            return Ok(userDto); 
        }
    }

}

