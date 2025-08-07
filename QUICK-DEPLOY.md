# 🚀 Quick Railway Deployment

Railway is the easiest option for deploying the Vault API with your Supabase database.

## Step 1: Deploy to Railway

1. **Visit**: [railway.app](https://railway.app)
2. **Sign up** with GitHub
3. **Click**: "Deploy from GitHub repo"
4. **Select**: Your repository
5. **Railway**: Auto-detects .NET and builds

## Step 2: Set Environment Variables

In Railway dashboard, go to your service > Variables tab:

```bash
DATABASE_URL=postgresql://postgres:Ram25shyam@#@db.obiyobfujaymoovwdfhg.supabase.co:5432/postgres
JWT_SECRET=your-super-secret-jwt-key-must-be-at-least-32-characters-long
ASPNETCORE_ENVIRONMENT=Production
```

## Step 3: Get Your API URL

After deployment, Railway provides a URL like:
```
https://vault-api-production-xxxx.up.railway.app
```

## Step 4: Update Frontend

Update `Vault.Web/wwwroot/appsettings.Production.json`:

```json
{
  "ApiBaseAddress": "https://your-actual-railway-url.up.railway.app/"
}
```

## Step 5: Deploy Frontend to Netlify

1. **Visit**: [netlify.com](https://netlify.com)
2. **Deploy**: "New site from Git"
3. **Build command**: `dotnet publish Vault.Web -c Release -o out`
4. **Publish directory**: `out/wwwroot`

## ✅ You're Done!

Your app is now deployed completely free:
- 🗄️ **Database**: Supabase (PostgreSQL)
- 🖥️ **API**: Railway
- 🌐 **Frontend**: Netlify

**Total Cost**: $0 (Railway's $5 credit covers small apps)

## 🔍 Monitoring

- **Railway**: Monitor API health and logs
- **Supabase**: Check database usage
- **Netlify**: Monitor frontend deployments

Perfect for production use! 🎉
