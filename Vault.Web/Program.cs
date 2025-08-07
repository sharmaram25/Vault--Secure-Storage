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

// Add Supabase service
builder.Services.AddScoped<SupabaseApiService>();

// Add Supabase authentication services
builder.Services.AddScoped<SupabaseAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<SupabaseAuthStateProvider>());
builder.Services.AddScoped<VaultAuthenticationService>();
builder.Services.AddScoped<SecretService>();

// Add authorization
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
