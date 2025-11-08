using FollowService.Dtos;
using FollowService.Services.FollowService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FollowService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] FollowDto dto)
        {
            dto.FollowerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(dto.FollowerId)) return Unauthorized();

            bool success = await _followService.FollowAsync(dto);
            if (!success) return BadRequest(new { message = "Cannot follow" });

            return Ok(new { message = "Followed successfully" });
        }

        [HttpDelete("unfollow")]
        public async Task<IActionResult> Unfollow([FromBody] FollowDto dto)
        {
            dto.FollowerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(dto.FollowerId)) return Unauthorized();

            bool success = await _followService.UnfollowAsync(dto);
            if (!success) return NotFound(new { message = "Not following" });

            return Ok(new { message = "Unfollowed successfully" });
        }

        [HttpGet("followers")]
        public async Task<IActionResult> GetFollowersCount()
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            int count = await _followService.GetFollowersCountAsync(UserId);
            return Ok(count);
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetFollowingCount()
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            int count = await _followService.GetFollowingCountAsync(UserId);
            return Ok(count);
        }
    }
}
