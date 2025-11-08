using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Dtos;
using UserService.Services.ProfileService;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _profileService.GetCurrentUserAsync(GetUserId());
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var result = await _profileService.UpdateProfileAsync(GetUserId(), dto);
            return result ? Ok("Profile updated successfully.") : NotFound("User not found.");
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = await _profileService.ChangePasswordAsync(GetUserId(), dto);
            if (!result) return BadRequest("Current password is incorrect.");
            return Ok("Password changed successfully.");
        }
    }
}

