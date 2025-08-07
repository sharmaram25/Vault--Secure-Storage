using System.Net.Http.Json;
using Vault.Core.DTOs;

namespace Vault.Web.Services;

public class SecretService
{
    private readonly HttpClient _httpClient;

    public SecretService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<SecretListDto>?> GetSecretsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SecretListDto>>("api/secrets");
        }
        catch
        {
            return null;
        }
    }

    public async Task<SecretDto?> GetSecretAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SecretDto>($"api/secrets/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<SecretListDto?> CreateSecretAsync(CreateSecretDto createSecretDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/secrets", createSecretDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SecretListDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SecretListDto?> UpdateSecretAsync(int id, UpdateSecretDto updateSecretDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/secrets/{id}", updateSecretDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SecretListDto>();
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
            var response = await _httpClient.DeleteAsync($"api/secrets/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
