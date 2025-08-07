# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Vault.API/Vault.API.csproj", "Vault.API/"]
COPY ["Vault.Core/Vault.Core.csproj", "Vault.Core/"]
COPY ["Vault.Infrastructure/Vault.Infrastructure.csproj", "Vault.Infrastructure/"]
COPY ["Vault.Web/Vault.Web.csproj", "Vault.Web/"]

# Restore dependencies
RUN dotnet restore "Vault.API/Vault.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the API project
WORKDIR "/src/Vault.API"
RUN dotnet build "Vault.API.csproj" -c Release -o /app/build

# Build the Web project
WORKDIR "/src/Vault.Web"
RUN dotnet build "Vault.Web.csproj" -c Release -o /app/build-web

# Publish both projects
WORKDIR "/src"
RUN dotnet publish "Vault.API/Vault.API.csproj" -c Release -o /app/publish --no-restore
RUN dotnet publish "Vault.Web/Vault.Web.csproj" -c Release -o /app/publish/wwwroot --no-restore

# Final stage - runtime image
FROM base AS final
WORKDIR /app

# Install necessary packages for production
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directory for SQLite database (works with both persistent and ephemeral storage)
RUN mkdir -p /app/data /tmp

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Vault.API.dll"]
