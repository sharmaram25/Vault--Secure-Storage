using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
using Vault.Core.DTOs;
using Xunit;

namespace Vault.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var registerDto = new UserRegistrationDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.Equal("testuser", authResponse.Username);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange - First register a user
        var registerDto = new UserRegistrationDto
        {
            Username = "logintest",
            Email = "logintest@example.com", 
            Password = "SecurePassword123!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new UserLoginDto
        {
            Username = "logintest",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.Equal("logintest", authResponse.Username);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto
        {
            Username = "nonexistent",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
