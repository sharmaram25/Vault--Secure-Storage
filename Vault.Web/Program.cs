using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Vault.Web;
using Vault.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HTTP client for Supabase
builder.Services.AddScoped(sp => new HttpClient());

// Add Supabase services
builder.Services.AddScoped<SupabaseApiService>();
builder.Services.AddScoped<SupabaseAuthStateProvider>();
builder.Services.AddScoped<VaultAuthenticationService>();
builder.Services.AddScoped<SecretService>();

// Add authentication services - Production Supabase Auth
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<SupabaseAuthStateProvider>());

// Add authorization
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
