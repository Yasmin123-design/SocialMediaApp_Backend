using ChatService.Dtos;
using ChatService.Services.ChatServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartChat([FromBody] StartChatRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid token or user not found" });

                request.UserA = userId;

                var threadDto = await _chatService.StartConversationAsync(request.UserA, request.UserB);
                return Ok(threadDto); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid token or user not found" });

                request.SenderId = userId;

                var msgDto = await _chatService.SendMessageAsync(
                    request.ThreadId,
                    request.SenderId,
                    request.Text
                );

                return Ok(msgDto); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("thread/{threadId}")]
        public async Task<IActionResult> GetThreadMessages(int threadId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid token or user not found" });

                var messagesDto = await _chatService.GetMessagesAsync(threadId, userId);
                return Ok(messagesDto); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("threads")]
        public async Task<IActionResult> GetUserThreads()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid token or user not found" });

                var threads = await _chatService.GetUserThreadsAsync(userId);

                return Ok(threads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
   
