using Vault.Core.DTOs;

namespace Vault.Web.Services;

public class VaultAuthenticationService
{
    private readonly SupabaseAuthStateProvider _authStateProvider;

    public VaultAuthenticationService(SupabaseAuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(UserLoginDto loginDto)
    {
        // Use username as email for Supabase (since Supabase uses email for auth)
        return await _authStateProvider.LoginAsync(loginDto.Username, loginDto.Password);
    }

    public async Task<bool> RegisterAsync(UserRegistrationDto registrationDto)
    {
        return await _authStateProvider.RegisterAsync(registrationDto.Email, registrationDto.Password, registrationDto.Username);
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.LogoutAsync();
    }
}
