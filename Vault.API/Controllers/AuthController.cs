using Microsoft.AspNetCore.Mvc;
using Vault.Core.DTOs;
using Vault.Core.Interfaces;
using Vault.Core.Models;

namespace Vault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] UserRegistrationDto registrationDto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(registrationDto.Username) ||
                string.IsNullOrWhiteSpace(registrationDto.Email) ||
                string.IsNullOrWhiteSpace(registrationDto.Password))
            {
                return BadRequest("Username, email, and password are required.");
            }

            // Check if user already exists
            if (await _userRepository.ExistsAsync(registrationDto.Username, registrationDto.Email))
            {
                return Conflict("User with this username or email already exists.");
            }

            // Hash password
            var passwordHash = _passwordHashingService.HashPassword(registrationDto.Password);

            // Create user
            var user = new User
            {
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                PasswordHash = passwordHash
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(createdUser.Id, createdUser.Username);

            var response = new AuthResponseDto
            {
                Token = token,
                Username = createdUser.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match JWT expiration
            };

            _logger.LogInformation("User {Username} registered successfully", createdUser.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto loginDto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Find user
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Verify password
            if (!_passwordHashingService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user.Id, user.Username);

            var response = new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match JWT expiration
            };

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "Internal server error");
        }
    }
}
