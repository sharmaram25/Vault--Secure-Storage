using System.Text.Json;
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
        _supabaseUrl = configuration["Supabase:Url"] ?? "";
        _supabaseKey = configuration["Supabase:AnonKey"] ?? "";
        
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

        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_supabaseUrl}/auth/v1/signup", content);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponse>(responseJson, _jsonOptions);
    }

    public async Task<AuthResponse?> SignInAsync(string email, string password)
    {
        var data = new { email, password };
        
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_supabaseUrl}/auth/v1/token?grant_type=password", content);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponse>(responseJson, _jsonOptions);
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
}

public class AuthResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public User? User { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

public class User
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
