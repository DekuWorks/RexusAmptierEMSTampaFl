Write-Host "Testing C# API..." -ForegroundColor Green

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5169/health" -Method GET
    Write-Host "✓ Health check passed: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "✗ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
} 