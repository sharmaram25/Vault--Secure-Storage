using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Vault.Web.Services;

public class SupabaseAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly SupabaseApiService _supabaseService;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public SupabaseAuthenticationStateProvider(SupabaseApiService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _supabaseService.SignInAsync(email, password);
            
            if (response?.AccessToken != null)
            {
                _supabaseService.SetAuthToken(response.AccessToken);
                
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, response.User?.Email ?? email),
                    new Claim(ClaimTypes.Email, response.User?.Email ?? email),
                    new Claim("access_token", response.AccessToken)
                };

                _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "supabase"));
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(string email, string password, string username)
    {
        try
        {
            var userData = new { username, email };
            var response = await _supabaseService.SignUpAsync(email, password, userData);
            
            return response != null && string.IsNullOrEmpty(response.Error);
        }
        catch
        {
            return false;
        }
    }

    public Task LogoutAsync()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }
}
