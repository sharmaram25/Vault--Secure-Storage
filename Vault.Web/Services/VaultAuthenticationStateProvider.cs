using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace Vault.Web.Services;

public class VaultAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;

    public VaultAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            
            // Set the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "vault_token", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "vault_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "vault_token");
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        var claims = keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
        
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
