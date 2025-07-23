# Test Login API Script
Write-Host "🧪 Testing EMS Login API..." -ForegroundColor Green

# Test health endpoint
Write-Host "`n1. Testing Health Endpoint..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5169/health" -Method GET
    Write-Host "✅ Health endpoint working: $($healthResponse | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Health endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test login endpoint
Write-Host "`n2. Testing Login Endpoint..." -ForegroundColor Yellow
$loginData = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5169/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "Token: $($loginResponse.data.token.Substring(0, 20))..." -ForegroundColor Cyan
    Write-Host "User: $($loginResponse.data.user.username)" -ForegroundColor Cyan
    Write-Host "Role: $($loginResponse.data.user.role)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`n🎉 Test completed!" -ForegroundColor Green 