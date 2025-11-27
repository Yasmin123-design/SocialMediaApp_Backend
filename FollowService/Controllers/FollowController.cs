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

        [HttpGet("countfollowersuser/{userId}")]
        public async Task<IActionResult> GetFollowersCount(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            int count = await _followService.GetFollowersCountAsync(userId);
            return Ok(count);
        }

        [HttpGet("countfollowinguser/{userId}")]
        public async Task<IActionResult> GetFollowingCount(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            int count = await _followService.GetFollowingCountAsync(userId);
            return Ok(count);
        }

        [HttpGet("followersusers/{userId}")]
        public async Task<IActionResult> GetFollowersUsers(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var users = await _followService.GetFollowersUsersAsync(userId);
            return Ok(users);
        }

        [HttpGet("followingusers/{userId}")]
        public async Task<IActionResult> GetFollowingUsers(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var users = await _followService.GetFollowingUsersAsync(userId);
            return Ok(users);
        }
    }
}
