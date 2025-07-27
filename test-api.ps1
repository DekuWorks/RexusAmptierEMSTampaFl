# Test the C# API login endpoint
Write-Host "Testing C# API login endpoint..." -ForegroundColor Green

try {
    $body = @{
        username = "admin"
        password = "pass123"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:5169/api/auth/login" -Method POST -ContentType "application/json" -Body $body
    
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor Yellow
}
catch {
    Write-Host "Error testing API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the C# API is running on port 5169" -ForegroundColor Yellow
} 