# Simple Backend Startup Script
Write-Host "🚀 Starting EMS Backend..." -ForegroundColor Green

# Navigate to API directory
Set-Location "RexusOps360.API"

# Kill any existing dotnet processes
Write-Host "🔄 Stopping any existing processes..." -ForegroundColor Yellow
taskkill /F /IM dotnet.exe 2>$null

# Wait a moment
Start-Sleep -Seconds 2

# Start the backend with HTTP only
Write-Host "🚀 Starting backend on HTTP only..." -ForegroundColor Green
Write-Host "📍 API will be available at: http://localhost:5169" -ForegroundColor Cyan
Write-Host ""

dotnet run --urls http://localhost:5169 --no-https 