using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace Vault.Web.Services;

public class SupabaseApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly JsonSerializerOptions _jsonOptions;

    public SupabaseApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _supabaseUrl = configuration["Supabase:Url"] ?? "https://obiyobfujaymoovwdfhg.supabase.co";
        _supabaseKey = configuration["Supabase:AnonKey"] ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im9iaXlvYmZ1amF5bW9vdndkZmhnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTQ1NTU3NjcsImV4cCI6MjA3MDEzMTc2N30.cEvCm-qe_RNDxVBbswI73dXVMfQgW9SitDKjcSoHwKU";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Set default headers
        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/{endpoint}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/{endpoint}", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"{_supabaseUrl}/rest/v1/{endpoint}", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/{endpoint}");
        response.EnsureSuccessStatusCode();
    }

    // Authentication methods using Supabase Auth
    public async Task<AuthResponse?> SignUpAsync(string email, string password, object? userData = null)
    {
        var data = new
        {
            email,
            password,
            data = userData ?? new { }
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Create a separate HttpClient for auth requests to avoid header conflicts
        using var authClient = new HttpClient();
        authClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        
        var response = await authClient.PostAsync($"{_supabaseUrl}/auth/v1/signup", content);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<AuthResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        
        return new AuthResponse 
        { 
            Error = $"HTTP {response.StatusCode}", 
            ErrorDescription = responseJson 
        };
    }

    public async Task<AuthResponse?> SignInAsync(string email, string password)
    {
        var data = new { 
            email, 
            password
        };
        
        // Use the correct JSON serialization for Supabase
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Create a separate HttpClient for auth requests to avoid header conflicts
        using var authClient = new HttpClient();
        authClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        
        var response = await authClient.PostAsync($"{_supabaseUrl}/auth/v1/signin", content);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<AuthResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        
        return new AuthResponse 
        { 
            Error = $"HTTP {response.StatusCode}", 
            ErrorDescription = responseJson 
        };
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
}

public class AuthResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("user")]
    public User? User { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
