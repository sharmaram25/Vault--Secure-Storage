using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text;
using Vault.API.Services;
using Vault.Core.Interfaces;
using Vault.Core.Services;
using Vault.Infrastructure.Data;
using Vault.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Railway deployment
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Vault API", 
        Version = "v1",
        Description = "Encrypted Vault - Secure Notes & Password Manager API"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Parse Supabase/PostgreSQL connection string
    var uri = new Uri(databaseUrl);
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.Trim('/');
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    
    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    
    builder.Services.AddDbContext<VaultDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use SQLite for local development
    builder.Services.AddDbContext<VaultDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=vault.db"));
}

// Repository registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISecretRepository, SecretRepository>();

// Service registration
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// JWT Authentication configuration
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "MyVerySecureJwtSecretKey123456789!";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "VaultAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "VaultWeb",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { 
                "https://localhost:5001", 
                "http://localhost:5000", 
                "http://localhost:5102", 
                "http://localhost:5173", 
                "https://localhost:7275",
                "https://vault-secure-storage.onrender.com",
                "https://vault-api.onrender.com",
                "https://*.up.railway.app",
                "https://*.netlify.app",
                "https://*.vercel.app",
                "https://*.azurestaticapps.net"
            };
            
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure static files to serve Blazor WebAssembly
app.UseDefaultFiles();
app.UseStaticFiles();

// Ensure database is created
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<VaultDbContext>();
        context.Database.EnsureCreated();
    }
}
catch (Exception ex)
{
    // Log the error but don't prevent the app from starting
    Console.WriteLine($"Database initialization error: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vault API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorWasm");
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", async (VaultDbContext context) => 
{
    try 
    {
        // Test database connectivity
        await context.Database.CanConnectAsync();
        return Results.Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = app.Environment.EnvironmentName,
            database = "connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { 
            status = "unhealthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = app.Environment.EnvironmentName,
            database = "disconnected",
            error = ex.Message
        }, statusCode: 503);
    }
});

app.MapControllers();

// Fallback routing for Blazor WebAssembly SPA
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
