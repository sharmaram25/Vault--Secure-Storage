# 🚀 Deployment Guide for Vault

This guide provides multiple deployment options for the Vault application.

## 🐳 Docker Deployment

### Prerequisites
- Docker installed on your system
- Git repository cloned

### Build and Run Locally
```bash
# Build the Docker image
docker build -t vault-app .

# Run the container
docker run -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e Jwt__SecretKey="YourSecure256BitSecretKeyHere" \
  -v vault-data:/app/data \
  vault-app
```

Access the application at `http://localhost:8080`

## 🌐 Render Deployment

### Step 1: Fork the Repository
1. Fork this repository to your GitHub account
2. Make sure all changes are committed and pushed

### Step 2: Deploy to Render
1. Go to [Render.com](https://render.com) and sign up/login
2. Click "New +" → "Web Service"
3. Connect your GitHub repository
4. Use these settings:
   - **Build Command**: (leave blank - uses Dockerfile)
   - **Start Command**: (leave blank - uses Dockerfile)
   - **Environment**: Docker
   - **Instance Type**: Free tier
   - **Auto-Deploy**: Yes

### Step 3: Environment Variables
Add these environment variables in Render dashboard:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000
Jwt__SecretKey=[Generate a secure 256-bit key]
Jwt__Issuer=VaultAPI
Jwt__Audience=VaultWeb
Jwt__ExpirationMinutes=60
```

### Step 4: Add Persistent Storage
1. In your service settings, go to "Disks"
2. Add a disk:
   - **Name**: vault-data
   - **Mount Path**: /app/data
   - **Size**: 1GB

## 🚄 Railway Deployment

### Step 1: Deploy to Railway
1. Go to [Railway.app](https://railway.app) and sign up/login
2. Click "New Project" → "Deploy from GitHub repo"
3. Select your Vault repository
4. Railway will automatically detect the `railway.toml` configuration

### Step 2: Set Environment Variables
Add these in Railway dashboard:
```
Jwt__SecretKey=[Generate a secure 256-bit key]
AllowedOrigins=https://your-app-name.up.railway.app
```

### Step 3: Add Volume (Optional)
1. Go to your service settings
2. Add a volume for persistent storage:
   - **Mount Path**: `/app/data`

## ☁️ Azure App Service Deployment

### Prerequisites
- Azure CLI installed
- Azure subscription

### Deployment Steps
```bash
# Login to Azure
az login

# Create resource group
az group create --name VaultResourceGroup --location "East US"

# Create App Service plan
az appservice plan create --name VaultPlan --resource-group VaultResourceGroup --sku F1 --is-linux

# Create web app
az webapp create --resource-group VaultResourceGroup --plan VaultPlan --name your-vault-app --deployment-container-image-name vault-app

# Configure app settings
az webapp config appsettings set --resource-group VaultResourceGroup --name your-vault-app --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  Jwt__SecretKey="YourSecure256BitSecretKey" \
  Jwt__Issuer=VaultAPI \
  Jwt__Audience=VaultWeb

# Deploy from container registry
az webapp deployment container config --name your-vault-app --resource-group VaultResourceGroup --docker-custom-image-name vault-app
```

## 🔑 Generating Secure JWT Secret

Use one of these methods to generate a secure 256-bit secret key:

### Method 1: Online Generator
Visit [Random.org](https://www.random.org/strings/) and generate:
- Length: 64 characters
- Character set: Alphanumeric + symbols

### Method 2: PowerShell (Windows)
```powershell
[System.Convert]::ToBase64String([System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes(32))
```

### Method 3: OpenSSL (Linux/Mac)
```bash
openssl rand -base64 32
```

### Method 4: Node.js
```javascript
require('crypto').randomBytes(32).toString('base64')
```

## 🛡️ Production Security Checklist

- [ ] **JWT Secret**: Use a cryptographically secure 256-bit key
- [ ] **HTTPS**: Ensure SSL/TLS is enabled (most platforms do this automatically)
- [ ] **Environment Variables**: Store all secrets in environment variables
- [ ] **CORS**: Update allowed origins for your domain
- [ ] **Database**: Use persistent storage for production data
- [ ] **Health Checks**: Verify `/health` endpoint is accessible
- [ ] **Logging**: Configure appropriate log levels for production
- [ ] **Rate Limiting**: Consider implementing rate limiting for API endpoints
- [ ] **Backup Strategy**: Plan for database backups if using persistent data

## 🌍 Custom Domain Setup

### Render
1. Go to your service settings
2. Navigate to "Custom Domains"
3. Add your domain and follow DNS configuration instructions

### Railway
1. Go to your service settings
2. Navigate to "Domains"
3. Add custom domain and configure DNS

### Azure
1. Go to App Service → Custom domains
2. Add domain and verify ownership
3. Configure SSL certificate

## 📊 Monitoring and Logs

### Render
- Access logs via Render dashboard → Your service → Logs
- Set up log drains for external monitoring

### Railway
- View logs in Railway dashboard → Your service → Deployments
- Configure log shipping to external services

### Azure
- Use Application Insights for monitoring
- Access logs via Azure portal → App Service → Log stream

## 🔧 Troubleshooting

### Common Issues

1. **Database Connection Error**
   - Ensure persistent storage is mounted correctly
   - Check file permissions in `/app/data`

2. **CORS Issues**
   - Verify `AllowedOrigins` includes your frontend URL
   - Check that both HTTP and HTTPS variants are included

3. **JWT Authentication Fails**
   - Verify `Jwt__SecretKey` is set and matches between API and any clients
   - Check token expiration settings

4. **Health Check Fails**
   - Ensure `/health` endpoint returns 200 status
   - Check application startup logs for errors

### Debug Commands
```bash
# Check container logs
docker logs <container_id>

# Access container shell
docker exec -it <container_id> /bin/bash

# Test health endpoint
curl -f http://localhost/health
```

## 📧 Support

If you encounter issues during deployment:
1. Check the application logs first
2. Verify all environment variables are set correctly
3. Ensure the database directory has write permissions
4. Contact the platform support if infrastructure issues persist

Remember to never commit sensitive information like JWT secrets to your repository!
