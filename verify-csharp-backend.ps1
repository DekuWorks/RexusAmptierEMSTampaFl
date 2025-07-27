# Verification script for C# backend
Write-Host "=== C# Backend Verification ===" -ForegroundColor Cyan

# Check if .NET is available
Write-Host "`n1. Checking .NET installation..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET not found" -ForegroundColor Red
    exit 1
}

# Check if C# project builds
Write-Host "`n2. Building C# project..." -ForegroundColor Green
try {
    Set-Location "RexusOps360.API"
    dotnet build --verbosity quiet
    Write-Host "✅ C# project builds successfully" -ForegroundColor Green
    Set-Location ".."
} catch {
    Write-Host "❌ C# project build failed" -ForegroundColor Red
    exit 1
}

# Check if server is running
Write-Host "`n3. Checking if server is running..." -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5169/health" -Method GET -TimeoutSec 5
    Write-Host "✅ Server is running on port 5169" -ForegroundColor Green
    Write-Host "   Health status: $($response | ConvertTo-Json)" -ForegroundColor Yellow
} catch {
    Write-Host "❌ Server not running on port 5169" -ForegroundColor Red
    Write-Host "   Start the server with: .\start-csharp-backend.ps1" -ForegroundColor Yellow
}

# Test login endpoint
Write-Host "`n4. Testing login endpoint..." -ForegroundColor Green
try {
    $loginBody = @{
        username = "admin"
        password = "pass123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5169/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody -TimeoutSec 10
    
    Write-Host "✅ Login endpoint working" -ForegroundColor Green
    Write-Host "   User: $($loginResponse.data.user.username)" -ForegroundColor Yellow
    Write-Host "   Role: $($loginResponse.data.user.role)" -ForegroundColor Yellow
} catch {
    Write-Host "❌ Login endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Check frontend files
Write-Host "`n5. Checking frontend configuration..." -ForegroundColor Green
$frontendFiles = @(
    "frontend/login.html",
    "frontend/register.html", 
    "frontend/index.html",
    "frontend/mobile-responder.html"
)

foreach ($file in $frontendFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        if ($content -match "localhost:5169") {
            Write-Host "✅ $file configured for C# API" -ForegroundColor Green
        } else {
            Write-Host "❌ $file needs API endpoint update" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ $file not found" -ForegroundColor Red
    }
}

# Check for remaining Python files
Write-Host "`n6. Checking for remaining Python files..." -ForegroundColor Green
$pythonFiles = Get-ChildItem -Path "." -Recurse -Include "*.py" -Exclude "venv" | Where-Object { $_.FullName -notlike "*venv*" }
if ($pythonFiles.Count -eq 0) {
    Write-Host "✅ No Python files found in main project" -ForegroundColor Green
} else {
    Write-Host "❌ Found $($pythonFiles.Count) Python files:" -ForegroundColor Red
    foreach ($file in $pythonFiles) {
        Write-Host "   - $($file.FullName)" -ForegroundColor Red
    }
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Cyan
Write-Host "🎯 C# Backend is ready for use!" -ForegroundColor Green 