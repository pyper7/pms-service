# PowerShell script to test email configuration
# Run this after updating your email settings

Write-Host "Testing TETFund PMS Email Configuration..." -ForegroundColor Green

# Test email endpoint
$baseUrl = "https://localhost:7000"
$testEmail = Read-Host "Enter your test email address"

# You'll need to get a JWT token first by logging in
Write-Host "First, you need to login to get a JWT token:" -ForegroundColor Yellow
Write-Host "POST $baseUrl/api/v1/auth/login" -ForegroundColor Cyan
Write-Host "Body: {`"emailOrStaffId`": `"hr@tetfund.gov.ng`", `"password`": `"password123`"}" -ForegroundColor Cyan

$token = Read-Host "Enter your JWT token (Bearer token only, without 'Bearer ')"

if ($token) {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $body = @{
        "to" = $testEmail
    } | ConvertTo-Json

    Write-Host "Sending test email..." -ForegroundColor Yellow
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/v1/emails/test" -Method POST -Headers $headers -Body $body
        Write-Host "Test email sent successfully!" -ForegroundColor Green
        Write-Host "Response: $($response.message)" -ForegroundColor Green
    }
    catch {
        Write-Host "Error sending test email:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }

    Write-Host "`nChecking email logs..." -ForegroundColor Yellow
    try {
        $logs = Invoke-RestMethod -Uri "$baseUrl/api/v1/emails/logs" -Method GET -Headers $headers
        Write-Host "Recent email logs:" -ForegroundColor Green
        $logs.data | Select-Object -First 5 | Format-Table To, Subject, Status, CreatedAt, ErrorMessage
    }
    catch {
        Write-Host "Error retrieving email logs:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
} else {
    Write-Host "No token provided. Please login first and get a JWT token." -ForegroundColor Red
}

Write-Host "`nEmail configuration test completed!" -ForegroundColor Green
