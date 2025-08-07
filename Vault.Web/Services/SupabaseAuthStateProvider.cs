using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Vault.Web.Services;

public class SupabaseAuthStateProvider : AuthenticationStateProvider
{
    private readonly SupabaseApiService _supabaseService;
    private readonly IJSRuntime _jsRuntime;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public SupabaseAuthStateProvider(SupabaseApiService supabaseService, IJSRuntime jsRuntime)
    {
        _supabaseService = supabaseService;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Try to get stored token from localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "supabase_token");
            
            if (!string.IsNullOrEmpty(token))
            {
                // Validate token and get user info
                _supabaseService.SetAuthToken(token);
                var userInfo = await GetUserInfoFromToken(token);
                
                if (userInfo != null)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userInfo.Id ?? ""),
                        new Claim(ClaimTypes.Name, userInfo.Email ?? ""),
                        new Claim(ClaimTypes.Email, userInfo.Email ?? ""),
                        new Claim("access_token", token)
                    };

                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "supabase"));
                }
            }
        }
        catch
        {
            // If anything fails, user is not authenticated
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }

        return new AuthenticationState(_currentUser);
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _supabaseService.SignInAsync(email, password);
            
            if (response?.AccessToken != null && response.User != null)
            {
                // Store token in localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "supabase_token", response.AccessToken);
                
                _supabaseService.SetAuthToken(response.AccessToken);
                
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, response.User.Id ?? ""),
                    new Claim(ClaimTypes.Name, response.User.Email ?? ""),
                    new Claim(ClaimTypes.Email, response.User.Email ?? ""),
                    new Claim("access_token", response.AccessToken)
                };

                _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "supabase"));
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RegisterAsync(string email, string password, string username)
    {
        try
        {
            var userData = new { username, email };
            var response = await _supabaseService.SignUpAsync(email, password, userData);
            
            if (response != null && string.IsNullOrEmpty(response.Error))
            {
                // If auto-confirmation is enabled, also login
                if (!string.IsNullOrEmpty(response.AccessToken))
                {
                    return await LoginWithToken(response.AccessToken, response.User);
                }
                return true; // Registration successful, might need email confirmation
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Remove token from localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "supabase_token");
            
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logout error: {ex.Message}");
        }
    }

    private async Task<bool> LoginWithToken(string token, User? user)
    {
        if (user == null) return false;

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "supabase_token", token);
        _supabaseService.SetAuthToken(token);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
            new Claim(ClaimTypes.Name, user.Email ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("access_token", token)
        };

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "supabase"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        
        return true;
    }

    private async Task<User?> GetUserInfoFromToken(string token)
    {
        try
        {
            // Parse JWT token to get user info (simplified)
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

            if (tokenData.TryGetProperty("sub", out var subElement) &&
                tokenData.TryGetProperty("email", out var emailElement))
            {
                return new User
                {
                    Id = subElement.GetString(),
                    Email = emailElement.GetString()
                };
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
