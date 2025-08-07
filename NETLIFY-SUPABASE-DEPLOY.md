# 🚀 Netlify + Supabase Deployment Guide

Deploy your Vault application using only Netlify (frontend) and Supabase (database + auth).

## Architecture
- **Netlify**: Hosts the Blazor WebAssembly frontend
- **Supabase**: Provides PostgreSQL database + built-in authentication API
- **No separate API server needed** - Use Supabase's REST API directly

## 🎯 Deployment Steps

### Step 1: Setup Supabase

1. **Create Supabase Project**:
   - Go to [supabase.com](https://supabase.com)
   - Sign up with GitHub
   - Create new project: `vault-secure-storage`
   - Choose region closest to you
   - Set a strong database password

2. **Get Supabase Credentials**:
   - Go to Settings > API
   - Copy these values:
     - **Project URL**: `https://your-project-ref.supabase.co`
     - **Anon/Public Key**: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

3. **Create Database Tables**:
   - Go to SQL Editor in Supabase
   - Run this SQL to create the tables:

```sql
-- Enable Row Level Security
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users table (extends Supabase auth.users)
CREATE TABLE public.user_profiles (
    id UUID REFERENCES auth.users(id) PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL
);

-- Secrets table
CREATE TABLE public.secrets (
    id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL, -- This will store encrypted data
    secret_type VARCHAR(50) NOT NULL DEFAULT 'note',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL
);

-- Enable Row Level Security
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.secrets ENABLE ROW LEVEL SECURITY;

-- RLS Policies for user_profiles
CREATE POLICY "Users can view own profile" ON public.user_profiles
    FOR SELECT USING (auth.uid() = id);

CREATE POLICY "Users can update own profile" ON public.user_profiles
    FOR UPDATE USING (auth.uid() = id);

CREATE POLICY "Users can insert own profile" ON public.user_profiles
    FOR INSERT WITH CHECK (auth.uid() = id);

-- RLS Policies for secrets
CREATE POLICY "Users can view own secrets" ON public.secrets
    FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can create own secrets" ON public.secrets
    FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update own secrets" ON public.secrets
    FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can delete own secrets" ON public.secrets
    FOR DELETE USING (auth.uid() = user_id);
```

### Step 2: Update Configuration

1. **Update Supabase Keys in your project**:
   - Edit `Vault.Web/wwwroot/appsettings.json`
   - Edit `Vault.Web/wwwroot/appsettings.Production.json`
   - Replace with your actual Supabase URL and Anon Key

### Step 3: Deploy to Netlify

1. **Go to [netlify.com](https://netlify.com)**
2. **Click "New site from Git"**
3. **Connect GitHub and select**: `sharmaram25/Vault--Secure-Storage`
4. **Configure build settings**:
   - **Branch**: `master`
   - **Build command**: `dotnet publish Vault.Web -c Release -o Vault.Web/bin/Release/net8.0/publish`
   - **Publish directory**: `Vault.Web/bin/Release/net8.0/publish/wwwroot`

5. **Environment Variables** (in Netlify dashboard):
   ```
   ASPNETCORE_ENVIRONMENT=Production
   DOTNET_VERSION=8.0.x
   ```

6. **Deploy**: Click "Deploy site"

## 🔧 How It Works

1. **Frontend**: Blazor WebAssembly runs entirely in the browser
2. **Authentication**: Uses Supabase Auth API directly
3. **Data**: Uses Supabase REST API with Row Level Security
4. **Encryption**: Client-side encryption before sending to Supabase
5. **Security**: Supabase handles authentication and database security

## 🚀 Benefits

- ✅ **100% Free** (within generous limits)
- ✅ **No server management** required
- ✅ **Automatic scaling**
- ✅ **Built-in authentication**
- ✅ **Real-time subscriptions** available
- ✅ **Global CDN** with Netlify
- ✅ **SSL/HTTPS** included

## 🔍 Testing

After deployment, your app will be available at:
- `https://your-app-name.netlify.app`

You can test:
1. User registration/login
2. Creating encrypted secrets
3. Viewing/editing secrets
4. Auto-logout functionality

## 📊 Free Tier Limits

**Supabase**:
- 2 projects
- 500MB database storage
- 50,000 monthly active users
- 2GB bandwidth/month
- 100MB file storage

**Netlify**:
- 300 build minutes/month
- 100GB bandwidth/month
- Unlimited sites

This setup is perfect for personal use and small teams!
