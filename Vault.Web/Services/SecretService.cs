using Vault.Core.DTOs;

namespace Vault.Web.Services;

public class SecretService
{
    private readonly SupabaseApiService _supabaseService;

    public SecretService(SupabaseApiService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<IEnumerable<SecretListDto>?> GetSecretsAsync()
    {
        try
        {
            var secrets = await _supabaseService.GetAsync<List<SupabaseSecret>>("secrets?select=id,title,created_at,updated_at");
            
            return secrets?.Select(s => new SecretListDto
            {
                Id = int.Parse(s.Id ?? "0"),
                Title = s.Title ?? "",
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            });
        }
        catch
        {
            return new List<SecretListDto>();
        }
    }

    public async Task<SecretDto?> GetSecretAsync(int id)
    {
        try
        {
            var secrets = await _supabaseService.GetAsync<List<SupabaseSecret>>($"secrets?id=eq.{id}&select=*");
            var secret = secrets?.FirstOrDefault();
            
            if (secret != null)
            {
                return new SecretDto
                {
                    Id = int.Parse(secret.Id ?? "0"),
                    Title = secret.Title ?? "",
                    Content = secret.Content ?? "",
                    CreatedAt = secret.CreatedAt,
                    UpdatedAt = secret.UpdatedAt
                };
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SecretListDto?> CreateSecretAsync(CreateSecretDto secretDto)
    {
        try
        {
            var supabaseSecret = new
            {
                title = secretDto.Title,
                content = secretDto.Content,
                secret_type = "note"
            };

            var result = await _supabaseService.PostAsync<List<SupabaseSecret>>("secrets", supabaseSecret);
            var createdSecret = result?.FirstOrDefault();
            
            if (createdSecret != null)
            {
                return new SecretListDto
                {
                    Id = int.Parse(createdSecret.Id ?? "0"),
                    Title = createdSecret.Title ?? "",
                    CreatedAt = createdSecret.CreatedAt,
                    UpdatedAt = createdSecret.UpdatedAt
                };
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SecretListDto?> UpdateSecretAsync(int id, UpdateSecretDto secretDto)
    {
        try
        {
            var supabaseSecret = new
            {
                title = secretDto.Title,
                content = secretDto.Content,
                secret_type = "note"
            };

            var result = await _supabaseService.PutAsync<List<SupabaseSecret>>($"secrets?id=eq.{id}", supabaseSecret);
            var updatedSecret = result?.FirstOrDefault();
            
            if (updatedSecret != null)
            {
                return new SecretListDto
                {
                    Id = int.Parse(updatedSecret.Id ?? "0"),
                    Title = updatedSecret.Title ?? "",
                    CreatedAt = updatedSecret.CreatedAt,
                    UpdatedAt = updatedSecret.UpdatedAt
                };
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteSecretAsync(int id)
    {
        try
        {
            await _supabaseService.DeleteAsync($"secrets?id=eq.{id}");
            return true;
        }
        catch
        {
            return false;
        }
    }
}

// Supabase response models
public class SupabaseSecret
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? SecretType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
