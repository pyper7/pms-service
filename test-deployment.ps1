# Deployment Test Script
# This script tests the deployed application endpoints

param(
    [Parameter(Mandatory=$true)]
    [string]$BaseUrl
)

Write-Host "Testing Deployment: $BaseUrl" -ForegroundColor Green

# Test endpoints
$endpoints = @(
    @{ Path = "/health"; Method = "GET"; Name = "Health Check" },
    @{ Path = "/swagger"; Method = "GET"; Name = "Swagger UI" },
    @{ Path = "/api/appraisal-settings/competencies"; Method = "GET"; Name = "Competencies API" }
)

foreach ($endpoint in $endpoints) {
    $url = $BaseUrl.TrimEnd('/') + $endpoint.Path
    Write-Host "`nTesting $($endpoint.Name)..." -ForegroundColor Cyan
    Write-Host "URL: $url" -ForegroundColor Gray
    
    try {
        $response = Invoke-WebRequest -Uri $url -Method $endpoint.Method -TimeoutSec 30
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ SUCCESS - Status: $($response.StatusCode)" -ForegroundColor Green
        } else {
            Write-Host "⚠️  WARNING - Status: $($response.StatusCode)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ FAILED - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n🎉 Deployment test completed!" -ForegroundColor Green
Write-Host "If all tests pass, your application is successfully deployed!" -ForegroundColor Yellow
