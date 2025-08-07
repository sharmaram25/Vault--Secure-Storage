namespace Vault.Core.Models;

public class Secret
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EncryptedContent { get; set; } = string.Empty;
    public string InitializationVector { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}
