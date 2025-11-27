using CommentService.Dtos;
using CommentService.Services.CommentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("getcommentsofpost/{postId}")]
        public async Task<IActionResult> GetCommentsOfPost(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            dto.UserId = userId;
            var result = await _commentService.AddCommentAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var deletedComment = await _commentService.DeleteCommentAsync(id, userId);

            if (deletedComment == null)
                return NotFound(new { message = "Comment not found or you are not the owner" });

            return Ok(deletedComment);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] string newContent)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var updated = await _commentService.UpdateCommentAsync(id, userId, newContent);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpGet("noofcommentsofpost/{postId}")]
        public async Task<IActionResult> GetNoOfCommentsOfPost(int postId)
        {
            var count = await _commentService.GetNoOfCommentsOfPostAsync(postId);
            return Ok(new { postId, count });
        }
    }
}

