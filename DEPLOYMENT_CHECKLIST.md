# 🚀 Deployment Checklist

## Pre-Deployment
- [x] Code committed to GitHub
- [x] Build successful (0 errors)
- [x] Dockerfile created
- [x] Environment variables configured
- [x] Database configuration ready

## Railway Deployment Steps

### 1. Create Railway Account
- [ ] Go to [railway.app](https://railway.app)
- [ ] Sign up with GitHub account
- [ ] Verify email if required

### 2. Deploy Application
- [ ] Click "New Project"
- [ ] Select "Deploy from GitHub repo"
- [ ] Choose repository: `pyper7/pms-service`
- [ ] Wait for build to complete (5-10 minutes)

### 3. Set up Database
- [ ] Railway will automatically create PostgreSQL database
- [ ] Note the database connection string from Railway dashboard

### 4. Configure Environment Variables
In Railway dashboard, go to Variables tab and add:

```bash
DATABASE_PROVIDER=PostgreSQL
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=PMS.API
JWT_AUDIENCE=PMS.Users
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7
ASPNETCORE_ENVIRONMENT=Production
```

### 5. Run Database Migrations
- [ ] Use Railway's built-in terminal or local migration script
- [ ] Run: `dotnet ef database update --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext`

### 6. Test Deployment
- [ ] Visit your Railway app URL
- [ ] Test API endpoints
- [ ] Verify database connectivity

## Alternative: Render Deployment

### 1. Create Render Account
- [ ] Go to [render.com](https://render.com)
- [ ] Sign up with GitHub account

### 2. Deploy Application
- [ ] Click "New +" → "Web Service"
- [ ] Connect GitHub repository: `pyper7/pms-service`
- [ ] Select branch: `master`
- [ ] Build command: `dotnet publish -c Release -o out`
- [ ] Start command: `dotnet out/PMS.API.dll`

### 3. Set up Database
- [ ] Create PostgreSQL database in Render
- [ ] Note connection string

### 4. Configure Environment Variables
Same as Railway configuration above.

## Testing Your Deployment

### API Endpoints to Test:
1. **Health Check:** `GET /health`
2. **Swagger UI:** `GET /swagger`
3. **Appraisal Settings:** `GET /api/appraisal-settings/competencies`

### Sample Test Commands:
```bash
# Health check
curl https://your-app.railway.app/health

# Test competencies endpoint
curl https://your-app.railway.app/api/appraisal-settings/competencies
```

## Troubleshooting

### Common Issues:
1. **Build Fails:** Check Dockerfile and dependencies
2. **Database Connection:** Verify connection string and environment variables
3. **Migration Fails:** Ensure database exists and is accessible
4. **App Won't Start:** Check logs in Railway/Render dashboard

### Getting Help:
- Railway: [docs.railway.app](https://docs.railway.app)
- Render: [render.com/docs](https://render.com/docs)
- Check application logs in hosting platform dashboard

## Success Indicators:
- [ ] Application builds successfully
- [ ] Database migrations run without errors
- [ ] API endpoints respond correctly
- [ ] Swagger UI loads properly
- [ ] Health check returns 200 OK
