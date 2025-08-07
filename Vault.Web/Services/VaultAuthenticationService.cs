using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Vault.Core.DTOs;

namespace Vault.Web.Services;

public class VaultAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public VaultAuthenticationService(
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> LoginAsync(UserLoginDto loginDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse != null)
                {
                    await SetTokenAsync(authResponse.Token);
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(UserRegistrationDto registrationDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registrationDto);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse != null)
                {
                    await SetTokenAsync(authResponse.Token);
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await RemoveTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task SetTokenAsync(string token)
    {
        // Store token in localStorage
        await ((VaultAuthenticationStateProvider)_authenticationStateProvider).SetTokenAsync(token);
        
        // Set authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task RemoveTokenAsync()
    {
        await ((VaultAuthenticationStateProvider)_authenticationStateProvider).ClearTokenAsync();
    }
}
