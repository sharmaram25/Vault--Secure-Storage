using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Vault.API.Services;
using Vault.Core.Interfaces;
using Vault.Core.Services;
using Vault.Infrastructure.Data;
using Vault.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<VaultDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=vault.db"));

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
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000", "http://localhost:5102", "http://localhost:5173", "https://localhost:7275")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VaultDbContext>();
    context.Database.EnsureCreated();
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
app.MapControllers();

app.Run();
