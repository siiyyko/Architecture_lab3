using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.DTOs;
using BCrypt.Net;
using BCrypt;
using Microsoft.CodeAnalysis.Scripting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserService.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserServiceContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(UserServiceContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _context.UserEntity.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            };

            _context.UserEntity.Add(userEntity);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = userEntity.Id,
                Email = userEntity.Email,
                UserName = userEntity.UserName
            };

            return CreatedAtAction(
                nameof(UsersController.GetUserEntity),
                new { controller = "Users", id = userEntity.Id },
                userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            var userEntity = await _context.UserEntity
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (userEntity == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, userEntity.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = GenerateJwtToken(userEntity);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(UserEntity user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key не налаштовано в appsettings.json");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}