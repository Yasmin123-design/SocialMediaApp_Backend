using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryShared.Dtos;
using UserService.Dtos;
using UserService.Models;
using LibraryShared.Services.UserClientService;
using UserService.helper;
using Microsoft.EntityFrameworkCore;

namespace UserService.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUserClientService _userClientService;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IUserClientService userClientService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _userClientService = userClientService;
        }
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var userDtos = users.Select(user => UserMapper.UserToDto(user)).ToList();

            return userDtos;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                Age = model.Age,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                MaritalStatus = model.MaritalStatus,
                Position = model.Position
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return await _userClientService.GetUserByIdAsync(user.Id);
        }

        public async Task<UserDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid credentials ❌");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var userDto = await _userClientService.GetUserByIdAsync(user.Id);
            userDto.Token = new JwtSecurityTokenHandler().WriteToken(token);
            userDto.Message = "User logged in successfully ✅";

            return userDto;
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    return UserMapper.UserToDto(user);
                }
            }
            return await _userClientService.GetUserByIdAsync(id);
        }

    }
}

