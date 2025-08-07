using System.ComponentModel.DataAnnotations;

namespace Vault.Core.DTOs;

public class UserRegistrationDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int TotalSecrets { get; set; }
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = string.Empty;
}
