namespace Vault.Core.DTOs;

public class SecretDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Decrypted content
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateSecretDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Plain text content to be encrypted
}

public class UpdateSecretDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Plain text content to be encrypted
}

public class SecretListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
