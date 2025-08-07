using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vault.Core.DTOs;
using Vault.Core.Interfaces;
using Vault.Infrastructure.Data;

namespace Vault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly VaultDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;

    public UserController(VaultDbContext context, IPasswordHashingService passwordHashingService)
    {
        _context = context;
        _passwordHashingService = passwordHashingService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            var secretCount = await _context.Secrets.CountAsync(s => s.UserId == userId);

            var profile = new UserProfileDto
            {
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                TotalSecrets = secretCount
            };

            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Verify current password
            if (!_passwordHashingService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Current password is incorrect");
            }

            // Hash new password
            user.PasswordHash = _passwordHashingService.HashPassword(changePasswordDto.NewPassword);
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
