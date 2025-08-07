# 🆓 Netlify + Supabase Deployment Guide

Deploy Vault with **100% FREE** hosting including persistent PostgreSQL database.

## 🎯 What You'll Get (All Free)
- ✅ **PostgreSQL Database** (500MB) via Supabase
- ✅ **Global CDN** with Netlify
- ✅ **Custom Domain** support
- ✅ **Automatic HTTPS**
- ✅ **Blazor WebAssembly** hosting
- ✅ **API backend** on Render

## 🚀 Deployment Steps

### Step 1: Setup Supabase Database

1. **Create Supabase Account**:
   - Go to [supabase.com](https://supabase.com)
   - Sign up with GitHub
   - Create new project: `vault-secure-storage`
   - Choose region closest to you

2. **Get Database Connection**:
   - Go to Settings > Database
   - Copy the connection string:
   ```
   postgresql://postgres:[YOUR-PASSWORD]@[PROJECT-REF].supabase.co:5432/postgres
   ```
   - Replace `[YOUR-PASSWORD]` with your database password

### Step 2: Deploy API (Choose One Option)

#### Option A: Railway (Recommended - Free $5 Credit)

1. **Create Railway Account**:
   - Go to [railway.app](https://railway.app)
   - Connect GitHub account

2. **Deploy from GitHub**:
   - Click "Deploy from GitHub repo"
   - Select your repository
   - Railway auto-detects .NET and builds automatically

3. **Add PostgreSQL Database**:
   - In Railway dashboard, click "New" > "Database" > "PostgreSQL"
   - Copy the DATABASE_URL from the database service

4. **Set Environment Variables**:
   ```
   DATABASE_URL=postgresql://postgres:Ram25shyam@#@db.obiyobfujaymoovwdfhg.supabase.co:5432/postgres
   JWT_SECRET=your-super-secret-jwt-key-must-be-at-least-32-characters-long
   ASPNETCORE_ENVIRONMENT=Production
   ```

#### Option B: Vercel (Serverless Functions)

1. **Create Vercel Account**:
   - Go to [vercel.com](https://vercel.com)
   - Connect GitHub account

2. **Deploy API as Functions**:
   - Import your repository
   - Vercel auto-detects and deploys
   - API endpoints become serverless functions

#### Option C: Azure Static Web Apps (Free Tier)

1. **Create Azure Account**:
   - Go to [azure.microsoft.com](https://azure.microsoft.com)
   - Get free $200 credit + free services

2. **Deploy Static Web App**:
   - Includes free API backend
   - Automatic CI/CD from GitHub
   - Custom domains included

### Step 3: Deploy Frontend to Netlify

1. **Update API Base URL**:
   - Edit `Vault.Web/wwwroot/appsettings.Production.json`
   - Set API URL based on your chosen option:
     - Railway: `https://your-app.up.railway.app/`
     - Vercel: `https://your-app.vercel.app/`
     - Azure: `https://your-app.azurestaticapps.net/`

2. **Deploy to Netlify**:
   - Go to [netlify.com](https://netlify.com)
   - Click "New site from Git"
   - Select your repository
   - Configure build:
     - **Build command**: `dotnet publish Vault.Web -c Release -o out`
     - **Publish directory**: `out/wwwroot`

3. **Set Environment Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ```

### Step 4: Configure CORS

Update `Vault.API/Program.cs` to allow your Netlify domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://your-app.netlify.app",
            "http://localhost:5000", // For local development
            "https://localhost:7147"  // For local development
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

## 🔧 Free Tier Limits

### Supabase (Database):
- 2 projects
- 500MB database storage
- 50,000 monthly active users
- 2GB bandwidth/month

### Railway (API - Recommended):
- $5/month free credit (enough for 24/7 operation)
- 512MB RAM
- 1GB disk space
- Custom domains

### Vercel (API Alternative):
- 100GB bandwidth/month
- 1000 serverless function invocations/day
- 10-second execution limit per function

### Azure Static Web Apps (API Alternative):
- 100GB bandwidth/month
- 2 custom domains
- Free SSL certificates
- Built-in authentication

### Netlify (Frontend):
- 300 build minutes/month
- 100GB bandwidth/month
- Unlimited sites
- Form submissions (100/month)

## 🚨 Important Notes

1. **Railway**: Uses $5 monthly credit (usually enough for small apps)
2. **Database**: Auto-creates tables via Entity Framework migrations
3. **HTTPS**: Enforced by default on all platforms
4. **Domains**: Get free subdomains on all platforms
5. **Cold Starts**: Only Vercel has cold starts (serverless), others are always on

## 🏆 Recommended Setup

**Best Free Option**: Netlify (Frontend) + Railway (API) + Supabase (Database)

- ✅ No cold starts
- ✅ Always-on API
- ✅ PostgreSQL database
- ✅ Custom domains
- ✅ Professional setup

## 🔍 Monitoring

- **Supabase**: Monitor database usage in dashboard
- **Railway**: Check service health, logs, and usage
- **Netlify**: Monitor bandwidth and build status

This setup provides a completely free, production-ready deployment!

```
DATABASE_URL=postgresql://[your-supabase-connection-string]
Jwt__SecretKey=PoDnC1KxkIRNlwPllGqIO3k/1Gxru3JDXYF3flPtVaY=
Jwt__Issuer=VaultAPI
Jwt__Audience=VaultWeb
ASPNETCORE_ENVIRONMENT=Production
```

## 🔥 Option 2: Firebase (Google) - Most Reliable

### Step 1: Setup Firebase

1. **Go to**: https://firebase.google.com
2. **Create project**: `vault-secure-storage`
3. **Enable Firestore Database**
4. **Enable Hosting**

### Step 2: Convert to Firebase

I'll create a Firebase version that uses Firestore for data storage.

## ⚡ Option 3: Railway (Has Free Tier)

Railway offers $5/month credit for free, which is enough for small apps:

1. **Go to**: https://railway.app
2. **Deploy from GitHub**
3. **Add PostgreSQL database** (free)
4. **No configuration needed** - uses our existing Docker setup

## 🌐 Option 4: Fly.io (Free Tier)

Fly.io offers generous free tier with persistent volumes:

1. **Go to**: https://fly.io
2. **Install flyctl CLI**
3. **Deploy with**: `fly deploy`
4. **Free tier includes**: 256MB RAM + 1GB storage

---

## 🏆 **My Recommendation: Railway**

Railway is the easiest because:
- ✅ Uses your existing Docker setup
- ✅ Automatic PostgreSQL database
- ✅ $5/month free credits (enough for small apps)
- ✅ Zero configuration changes needed
- ✅ One-click deployment

### Quick Railway Setup:

1. **Visit**: https://railway.app
2. **Connect GitHub**
3. **Deploy**: `sharmaram25/Vault--Secure-Storage`
4. **Add PostgreSQL** from dashboard
5. **Set environment variables**:
   ```
   Jwt__SecretKey=PoDnC1KxkIRNlwPllGqIO3k/1Gxru3JDXYF3flPtVaY=
   ```

That's it! Railway will:
- Automatically build using your Dockerfile
- Provide a PostgreSQL database
- Give you a public URL
- Handle HTTPS automatically

Would you like me to help you set up Railway, or would you prefer to try one of the other options?
