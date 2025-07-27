# Comprehensive test script for C# API endpoints
Write-Host "=== Testing C# API Endpoints ===" -ForegroundColor Cyan

$baseUrl = "http://localhost:5169/api"

# Test 1: Health Check
Write-Host "`n1. Testing Health Check..." -ForegroundColor Green
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5169/health" -Method GET
    Write-Host "✓ Health check passed: $($health | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "✗ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Auth Login
Write-Host "`n2. Testing Auth Login..." -ForegroundColor Green
try {
    $loginBody = @{
        username = "admin"
        password = "pass123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body $loginBody
    
    Write-Host "✓ Login successful!" -ForegroundColor Green
    Write-Host "  Token: $($loginResponse.data.token)" -ForegroundColor Yellow
    Write-Host "  User: $($loginResponse.data.user.username) - $($loginResponse.data.user.role)" -ForegroundColor Yellow
    
    $token = $loginResponse.data.token
} catch {
    Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    $token = $null
}

# Test 3: Get Current User (if login successful)
if ($token) {
    Write-Host "`n3. Testing Get Current User..." -ForegroundColor Green
    try {
        $headers = @{
            "Authorization" = "Bearer $token"
        }
        
        $userResponse = Invoke-RestMethod -Uri "$baseUrl/auth/me" -Method GET -Headers $headers
        Write-Host "✓ Get current user successful: $($userResponse.data.username)" -ForegroundColor Green
    } catch {
        Write-Host "✗ Get current user failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 4: Dashboard Stats
Write-Host "`n4. Testing Dashboard Stats..." -ForegroundColor Green
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $statsResponse = Invoke-RestMethod -Uri "$baseUrl/dashboard/stats" -Method GET -Headers $headers
    Write-Host "✓ Dashboard stats retrieved successfully" -ForegroundColor Green
    Write-Host "  Stats: $($statsResponse | ConvertTo-Json -Depth 2)" -ForegroundColor Yellow
} catch {
    Write-Host "✗ Dashboard stats failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Incidents
Write-Host "`n5. Testing Incidents..." -ForegroundColor Green
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $incidentsResponse = Invoke-RestMethod -Uri "$baseUrl/incidents" -Method GET -Headers $headers
    Write-Host "✓ Incidents retrieved successfully" -ForegroundColor Green
    Write-Host "  Count: $($incidentsResponse.data.Count)" -ForegroundColor Yellow
} catch {
    Write-Host "✗ Incidents failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Responders
Write-Host "`n6. Testing Responders..." -ForegroundColor Green
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $respondersResponse = Invoke-RestMethod -Uri "$baseUrl/responders" -Method GET -Headers $headers
    Write-Host "✓ Responders retrieved successfully" -ForegroundColor Green
    Write-Host "  Count: $($respondersResponse.data.Count)" -ForegroundColor Yellow
} catch {
    Write-Host "✗ Responders failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Equipment
Write-Host "`n7. Testing Equipment..." -ForegroundColor Green
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $equipmentResponse = Invoke-RestMethod -Uri "$baseUrl/equipment" -Method GET -Headers $headers
    Write-Host "✓ Equipment retrieved successfully" -ForegroundColor Green
    Write-Host "  Count: $($equipmentResponse.data.Count)" -ForegroundColor Yellow
} catch {
    Write-Host "✗ Equipment failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 8: Notifications
Write-Host "`n8. Testing Notifications..." -ForegroundColor Green
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $notificationsResponse = Invoke-RestMethod -Uri "$baseUrl/notifications" -Method GET -Headers $headers
    Write-Host "✓ Notifications retrieved successfully" -ForegroundColor Green
    Write-Host "  Count: $($notificationsResponse.data.Count)" -ForegroundColor Yellow
} catch {
    Write-Host "✗ Notifications failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 9: Weather
Write-Host "`n9. Testing Weather..." -ForegroundColor Green
try {
    $weatherResponse = Invoke-RestMethod -Uri "$baseUrl/weather" -Method GET
    Write-Host "✓ Weather retrieved successfully" -ForegroundColor Green
    Write-Host "  Weather: $($weatherResponse | ConvertTo-Json)" -ForegroundColor Yellow
} catch {
    Write-Host "✗ Weather failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== API Testing Complete ===" -ForegroundColor Cyan 