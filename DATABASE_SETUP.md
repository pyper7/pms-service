# Database Setup Guide

This application supports both SQL Server and PostgreSQL databases. You can configure which database to use through the `Database:Provider` setting in your configuration files.

## Supported Databases

- **SQL Server** (Default)
- **PostgreSQL**

## Configuration

### 1. Database Provider Selection

Set the `Database:Provider` in your configuration:

```json
{
  "Database": {
    "Provider": "SqlServer"  // or "PostgreSQL"
  }
}
```

### 2. Connection Strings

#### SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=server; Initial Catalog=database; User Id=user; Password=password; Integrated Security=False; MultipleActiveResultSets=True; Persist Security Info=True; TrustServerCertificate=True"
  }
}
```

#### PostgreSQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pms_db;Username=user;Password=password;Port=5432;SslMode=Require;TrustServerCertificate=true;"
  }
}
```

## Environment Variables

For production deployments, you can use environment variables:

### SQL Server
```bash
DATABASE_PROVIDER=SqlServer
CONNECTION_STRING="Data Source=server; Initial Catalog=database; User Id=user; Password=password; Integrated Security=False; MultipleActiveResultSets=True; Persist Security Info=True; TrustServerCertificate=True"
```

### PostgreSQL
```bash
DATABASE_PROVIDER=PostgreSQL
CONNECTION_STRING="Host=localhost;Database=pms_db;Username=user;Password=password;Port=5432;SslMode=Require;TrustServerCertificate=true;"
```

## Migration Commands

### Using PowerShell Script
```powershell
# For SQL Server (default)
.\migrate-database.ps1

# For PostgreSQL
.\migrate-database.ps1 -DatabaseProvider PostgreSQL

# With custom connection string
.\migrate-database.ps1 -DatabaseProvider PostgreSQL -ConnectionString "Host=localhost;Database=pms_db;Username=user;Password=password;"
```

### Manual Migration Commands

#### SQL Server
```bash
# Set environment variable
$env:DATABASE_PROVIDER = "SqlServer"

# Add migration
dotnet ef migrations add InitialCreate --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext

# Apply migration
dotnet ef database update --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext
```

#### PostgreSQL
```bash
# Set environment variable
$env:DATABASE_PROVIDER = "PostgreSQL"

# Add migration
dotnet ef migrations add InitialCreate --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext

# Apply migration
dotnet ef database update --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext
```

## Free Database Hosting Options

### PostgreSQL (Recommended for free hosting)
- **Railway**: Offers free PostgreSQL database
- **Render**: Free PostgreSQL tier available
- **Supabase**: Free PostgreSQL database
- **Neon**: Free PostgreSQL database
- **ElephantSQL**: Free PostgreSQL database

### SQL Server
- **Azure SQL Database**: Free tier available (limited)
- **AWS RDS**: Free tier available (limited)

## Switching Between Databases

1. **Update Configuration**: Change `Database:Provider` in your configuration
2. **Update Connection String**: Provide the appropriate connection string
3. **Run Migrations**: Execute the migration script for the new database
4. **Test**: Verify the application works with the new database

## Database-Specific Features

### PostgreSQL Advantages
- Better support for JSON data types
- More advanced indexing options
- Better performance for complex queries
- More hosting options for free tiers
- Better support for array data types

### SQL Server Advantages
- Better integration with .NET ecosystem
- More familiar to Windows developers
- Better tooling support
- Better performance for OLTP workloads

## Troubleshooting

### Common Issues

1. **Migration Fails**: Ensure the database exists and the connection string is correct
2. **Provider Not Found**: Make sure the correct NuGet packages are installed
3. **Connection Timeout**: Check network connectivity and firewall settings
4. **Authentication Failed**: Verify credentials and permissions

### Debugging

Enable detailed logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## Production Considerations

1. **Connection Pooling**: Configure appropriate connection pool sizes
2. **Backup Strategy**: Implement regular database backups
3. **Monitoring**: Set up database monitoring and alerting
4. **Security**: Use encrypted connections and strong authentication
5. **Performance**: Monitor and optimize query performance
