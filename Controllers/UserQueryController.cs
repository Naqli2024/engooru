using Engooru.Data;
using Engooru.DTOs.UserQueryDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Engooru.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserQueryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserQueryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Mobile = u.Mobile,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsVerified = u.IsVerified
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Allow only self or admin
            if (role != "Admin" && tokenUserId != id.ToString())
                return Forbid("You can only access your own data");

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Mobile = u.Mobile,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsVerified = u.IsVerified
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully");
        }
    }
}