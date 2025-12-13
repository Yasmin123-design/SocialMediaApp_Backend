using FeedService.Dtos;
using FeedService.Services.FeedService;
using LibraryShared.Services.UserClientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;
        private readonly IUserClientService _userClient;

        public FeedController(IFeedService feedService , 
            IUserClientService userClient
            )
        {
            _feedService = feedService;
            _userClient = userClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _feedService.GetAllAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var post = await _feedService.GetByIdAsync(id);
            return Ok(post);
        }

        [HttpGet("userposts/{userId}")]
        public async Task<IActionResult> GetUserPosts(string userId)
        {
            var post = await _feedService.GetUserFeedAsync(userId);
            return Ok(post);
        }

        [HttpPost]
        [RequestSizeLimit(2147483648)] 
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        public async Task<IActionResult> Create([FromForm] CreateFeedPostDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });

            var created = await _feedService.CreateAsync(dto, userId);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFeedPostDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });
            var updated = await _feedService.UpdateAsync(id, dto, userId);

            if (!updated.success)
                return BadRequest(new { message = updated.error });

            return Ok(new { message = "Post updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _feedService.DeleteAsync(id);
            return Ok(new { message = "Post deleted successfully" });

        }
        [HttpGet("countpostsofuser/{userId}")]
        public async Task<ActionResult<int>> GetUserPostsCount(string userId)
        {

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });

            var count = await _feedService.GetUserPostsCountAsync(userId);
            return Ok(count);
        }
        [HttpPost("save/{postId}")]
        public async Task<IActionResult> SavePost(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });

            var result = await _feedService.SavePostAsync(userId, postId);

            if (!result.success)
                return NotFound(new { error = result.error });

            return Ok(new { message = "Post saved successfully" });
        }


        [HttpGet("savedposts")]
        public async Task<IActionResult> GetSavedPosts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });

            var savedPosts = await _feedService.GetSavedPostsAsync(userId);
            return Ok(savedPosts);
        }
        [HttpDelete("unsave/{postId}")]
        public async Task<IActionResult> RemoveSavedPost(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });

            var result = await _feedService.RemoveSavedPostAsync(userId, postId);

            if (!result.success)
                return NotFound(new { error = result.error });

            return Ok(new { message = "Post removed from saved successfully" });
        }

        [HttpPost("share/{postId}")]
        public async Task<IActionResult> SharePost(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not found" });

            var userExists = await _userClient.CheckUserExistsAsync(userId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist" });

            var result = await _feedService.SharePostAsync(userId, postId);

            if (!result.success)
                return NotFound(new { error = result.error });

            return Ok(new { message = "Post shared successfully" });
        }

    }
}

