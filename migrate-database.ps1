# Database Migration Script
# This script can migrate the database for both SQL Server and PostgreSQL

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("SqlServer", "PostgreSQL")]
    [string]$DatabaseProvider = "SqlServer",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "",
    
    [Parameter(Mandatory=$false)]
    [string]$MigrationName = "InitialCreate"
)

Write-Host "Database Migration Script" -ForegroundColor Green
Write-Host "Database Provider: $DatabaseProvider" -ForegroundColor Yellow

# Set environment variable for database provider
$env:DATABASE_PROVIDER = $DatabaseProvider

if ($ConnectionString) {
    $env:CONNECTION_STRING = $ConnectionString
}

try {
    # Add initial migration
    Write-Host "Adding migration: $MigrationName" -ForegroundColor Cyan
    dotnet ef migrations add $MigrationName --project PMS.Infrastructure --startup-project PMS.API --context PmsDbContext
    
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
