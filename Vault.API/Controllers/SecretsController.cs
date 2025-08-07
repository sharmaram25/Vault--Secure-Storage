using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vault.Core.DTOs;
using Vault.Core.Interfaces;
using Vault.Core.Models;

namespace Vault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecretsController : ControllerBase
{
    private readonly ISecretRepository _secretRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SecretsController> _logger;

    public SecretsController(
        ISecretRepository secretRepository,
        IEncryptionService encryptionService,
        ILogger<SecretsController> logger)
    {
        _secretRepository = secretRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SecretListDto>>> GetSecrets()
    {
        try
        {
            var userId = GetCurrentUserId();
            var secrets = await _secretRepository.GetByUserIdAsync(userId);

            var secretDtos = secrets.Select(s => new SecretListDto
            {
                Id = s.Id,
                Title = s.Title,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            });

            return Ok(secretDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secrets");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SecretDto>> GetSecret(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var secret = await _secretRepository.GetByIdAsync(id, userId);

            if (secret == null)
            {
                return NotFound("Secret not found");
            }

            // Decrypt the content
            var decryptedContent = _encryptionService.Decrypt(secret.EncryptedContent, secret.InitializationVector);

            var secretDto = new SecretDto
            {
                Id = secret.Id,
                Title = secret.Title,
                Content = decryptedContent,
                CreatedAt = secret.CreatedAt,
                UpdatedAt = secret.UpdatedAt
            };

            return Ok(secretDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret {SecretId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SecretListDto>> CreateSecret([FromBody] CreateSecretDto createSecretDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createSecretDto.Title) || string.IsNullOrWhiteSpace(createSecretDto.Content))
            {
                return BadRequest("Title and content are required");
            }

            var userId = GetCurrentUserId();

            // Encrypt the content
            var (encryptedContent, iv) = _encryptionService.Encrypt(createSecretDto.Content);

            var secret = new Secret
            {
                Title = createSecretDto.Title,
                EncryptedContent = encryptedContent,
                InitializationVector = iv,
                UserId = userId
            };

            var createdSecret = await _secretRepository.CreateAsync(secret);

            var secretDto = new SecretListDto
            {
                Id = createdSecret.Id,
                Title = createdSecret.Title,
                CreatedAt = createdSecret.CreatedAt,
                UpdatedAt = createdSecret.UpdatedAt
            };

            _logger.LogInformation("Secret {SecretId} created for user {UserId}", createdSecret.Id, userId);
            return CreatedAtAction(nameof(GetSecret), new { id = createdSecret.Id }, secretDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SecretListDto>> UpdateSecret(int id, [FromBody] UpdateSecretDto updateSecretDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateSecretDto.Title) || string.IsNullOrWhiteSpace(updateSecretDto.Content))
            {
                return BadRequest("Title and content are required");
            }

            var userId = GetCurrentUserId();
            var secret = await _secretRepository.GetByIdAsync(id, userId);

            if (secret == null)
            {
                return NotFound("Secret not found");
            }

            // Encrypt the new content
            var (encryptedContent, iv) = _encryptionService.Encrypt(updateSecretDto.Content);

            secret.Title = updateSecretDto.Title;
            secret.EncryptedContent = encryptedContent;
            secret.InitializationVector = iv;

            var updatedSecret = await _secretRepository.UpdateAsync(secret);

            var secretDto = new SecretListDto
            {
                Id = updatedSecret.Id,
                Title = updatedSecret.Title,
                CreatedAt = updatedSecret.CreatedAt,
                UpdatedAt = updatedSecret.UpdatedAt
            };

            _logger.LogInformation("Secret {SecretId} updated for user {UserId}", id, userId);
            return Ok(secretDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating secret {SecretId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSecret(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _secretRepository.DeleteAsync(id, userId);

            if (!deleted)
            {
                return NotFound("Secret not found");
            }

            _logger.LogInformation("Secret {SecretId} deleted for user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret {SecretId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
