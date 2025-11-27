using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Services.AuthService;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userDto = await _authService.RegisterAsync(model);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
           }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userDto = await _authService.LoginAsync(model);
                return Ok(userDto); 
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var userDto = await _authService.GetUserByIdAsync(id);
            if (userDto == null)
                return NotFound(new { Message = "User not found." });

            return Ok(userDto); 
        }

        [HttpGet("getallusers")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();

            if (users == null || !users.Any())
                return NotFound(new { Message = "No users found." });

            return Ok(users);
        }
    }

}

