# PMS Service Deployment Guide

This guide will help you deploy the Performance Management System (PMS) API to various free hosting platforms.

## Prerequisites

- GitHub account
- .NET 9 SDK (for local development)
- Docker (optional, for containerized deployment)
- Database (SQL Server or PostgreSQL) - see [DATABASE_SETUP.md](DATABASE_SETUP.md) for details

## Database Setup

This application supports both SQL Server and PostgreSQL databases. For free hosting, PostgreSQL is recommended as it has more free tier options available.

### PostgreSQL (Recommended for Free Hosting)
- **Railway**: Offers free PostgreSQL database
- **Render**: Free PostgreSQL tier available
- **Supabase**: Free PostgreSQL database
- **Neon**: Free PostgreSQL database

### SQL Server
- **Azure SQL Database**: Free tier available (limited)

See [DATABASE_SETUP.md](DATABASE_SETUP.md) for detailed database configuration instructions.

## Free Hosting Options

### 1. Railway (Recommended)

Railway offers the easiest deployment with automatic GitHub integration.

#### Steps:

1. **Sign up at [Railway.app](https://railway.app)**
2. **Connect your GitHub account**
3. **Create a new project**
4. **Add your repository**: `https://github.com/pyper7/pms-service.git`
5. **Add PostgreSQL database**:
   - Click "New" → "Database" → "PostgreSQL"
   - Railway will automatically provide connection string
6. **Set environment variables**:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection` (from database)
   - `JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!`
   - `SMTP_SERVER=smtp.gmail.com`
   - `SENDER_EMAIL=your-email@gmail.com`
   - `SENDER_NAME=TETFund PMS`
   - `SMTP_USERNAME=your-email@gmail.com`
   - `SMTP_PASSWORD=your-app-password`
   - `BASE_URL=https://your-app.railway.app`
7. **Deploy**: Railway will automatically build and deploy your app

#### Railway Environment Variables:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
SMTP_SERVER=smtp.gmail.com
SENDER_EMAIL=your-email@gmail.com
SENDER_NAME=TETFund PMS
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
BASE_URL=https://your-app.railway.app
```

### 2. Render

Render provides free hosting with PostgreSQL database.

#### Steps:

1. **Sign up at [Render.com](https://render.com)**
2. **Connect your GitHub account**
3. **Create a new Web Service**
4. **Connect repository**: `https://github.com/pyper7/pms-service.git`
5. **Configure build settings**:
   - Build Command: `cd PMS.API && dotnet publish -c Release -o ./publish`
   - Start Command: `cd PMS.API/publish && dotnet PMS.API.dll`
6. **Add PostgreSQL database**:
   - Create a new PostgreSQL database
   - Note the connection string
7. **Set environment variables** (same as Railway)
8. **Deploy**

### 3. Fly.io

Fly.io offers a generous free tier with PostgreSQL.

#### Steps:

1. **Sign up at [Fly.io](https://fly.io)**
2. **Install flyctl CLI**:
   ```bash
   # Windows (PowerShell)
   iwr https://fly.io/install.ps1 -useb | iex
   
   # macOS/Linux
   curl -L https://fly.io/install.sh | sh
   ```
3. **Login to Fly.io**:
   ```bash
   fly auth login
   ```
4. **Create PostgreSQL database**:
   ```bash
   fly postgres create --name pms-db
   ```
5. **Deploy your app**:
   ```bash
   fly deploy
   ```

## Environment Variables

Set these environment variables in your hosting platform:

### Required Variables:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection` (database connection string)
- `JWT_SECRET_KEY` (at least 32 characters)
- `BASE_URL` (your app's URL)

### Email Configuration (Optional):
- `SMTP_SERVER` (e.g., smtp.gmail.com)
- `SENDER_EMAIL`
- `SENDER_NAME`
- `SMTP_USERNAME`
- `SMTP_PASSWORD`

## Database Setup

The application will automatically:
1. Create the database schema on first run
2. Seed initial data (users, roles, permissions)
3. Run migrations

## API Endpoints

Once deployed, your API will be available at:

- **Swagger UI**: `https://your-app-url/swagger`
- **Health Check**: `https://your-app-url/health`
- **API Base**: `https://your-app-url/api/v1`

### Main API Endpoints:

#### Authentication:
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh token
- `GET /api/v1/auth/profile` - Get user profile

#### Appraisal Settings:
- `GET /api/v1/appraisal-settings/competencies` - Get competencies
- `POST /api/v1/appraisal-settings/competencies` - Create competency
- `GET /api/v1/appraisal-settings/processes` - Get processes
- `GET /api/v1/appraisal-settings/periods` - Get appraisal periods
- `GET /api/v1/appraisal-settings/scoring-weights` - Get scoring weights
- `GET /api/v1/appraisal-settings/notification-settings` - Get notification settings

## Testing Your Deployment

1. **Check health**: Visit `https://your-app-url/health`
2. **View API docs**: Visit `https://your-app-url/swagger`
3. **Test authentication**: Use the login endpoint
4. **Test appraisal settings**: Use the competencies endpoint

## Troubleshooting

### Common Issues:

1. **Database connection failed**:
   - Check connection string format
   - Ensure database is accessible from your app

2. **JWT token issues**:
   - Verify JWT_SECRET_KEY is set
   - Ensure it's at least 32 characters long

3. **Email not working**:
   - Check SMTP credentials
   - Verify app password for Gmail

4. **Build failures**:
   - Check .NET version compatibility
   - Verify all dependencies are restored

### Logs:
- **Railway**: Check logs in the dashboard
- **Render**: View logs in the service dashboard
- **Fly.io**: Use `fly logs` command

## Security Considerations

1. **Change default JWT secret** in production
2. **Use strong database passwords**
3. **Enable HTTPS** (most platforms do this automatically)
4. **Set up proper CORS** policies
5. **Use environment variables** for sensitive data

## Monitoring

Most platforms provide:
- Application logs
- Performance metrics
- Error tracking
- Uptime monitoring

## Support

For issues with:
- **Railway**: [Railway Support](https://railway.app/help)
- **Render**: [Render Support](https://render.com/support)
- **Fly.io**: [Fly.io Community](https://community.fly.io/)

For application-specific issues, check the GitHub repository issues.
