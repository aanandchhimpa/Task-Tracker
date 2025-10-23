using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Helpers;
using TaskManagement.Api.Services;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly TaskManagementContext _context;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public AccountController(TaskManagementContext context, ITokenService tokenService, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _tokenService = tokenService;
            _jwtSettings = jwtOptions.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { error = "Email already in use" });

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DepartmentId = dto.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            return Created("", new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized(new { error = "Invalid credentials" });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials" });

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            return Ok(new { id = user.Id, email = user.Email, name = user.FirstName + " " + user.LastName });
        }
    }
}