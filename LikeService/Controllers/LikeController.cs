using LikeService.Dtos;
using LikeService.Services.LikeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LikeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddLike([FromBody] LikeDto dto)
        {
            dto.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(dto.UserId)) return Unauthorized();

            bool added = await _likeService.AddLikeAsync(dto);
            if (!added) return BadRequest(new { message = "Already liked" });

            return Ok(new { message = "Liked successfully" });
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveLike([FromBody] LikeDto dto)
        {
            dto.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(dto.UserId)) return Unauthorized();

            bool removed = await _likeService.RemoveLikeAsync(dto);
            if (!removed) return NotFound();

            return Ok(new { message = "Unliked successfully" });
        }

        [HttpGet("count/{postId}")]
        public async Task<IActionResult> GetCount(int postId)
        {
            int count = await _likeService.GetLikesCountAsync(postId);
            return Ok(count);
        }
    }
}

