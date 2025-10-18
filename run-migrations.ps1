# Database Migration Runner
# This script runs database migrations for the deployed application

param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseProvider = "PostgreSQL"
)

Write-Host "Running Database Migrations..." -ForegroundColor Green
Write-Host "Database Provider: $DatabaseProvider" -ForegroundColor Yellow
Write-Host "Connection String: $($ConnectionString.Substring(0, 50))..." -ForegroundColor Yellow

# Set environment variables
$env:DATABASE_PROVIDER = $DatabaseProvider
$env:CONNECTION_STRING = $ConnectionString
$env:ASPNETCORE_ENVIRONMENT = "Production"

try {
    # Add migration if needed
    Write-Host "Adding migration..." -ForegroundColor Cyan
    dotnet ef migrations add InitialCreate --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration added successfully!" -ForegroundColor Green
        
        # Apply migration
        Write-Host "Applying migration to database..." -ForegroundColor Cyan
        dotnet ef database update --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Database migration completed successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to apply migration to database." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Failed to add migration." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error during migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "Migration process completed!" -ForegroundColor Green
