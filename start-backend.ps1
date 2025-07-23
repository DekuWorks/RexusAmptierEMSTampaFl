# EMS Tampa-FL Amptier Backend Startup Script
Write-Host "🚀 Starting EMS Tampa-FL Amptier Backend..." -ForegroundColor Green

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET is not installed. Please install .NET 8.0 or later." -ForegroundColor Red
    exit 1
}

# Navigate to the API directory
Set-Location "RexusOps360.API"

# Trust HTTPS certificate for development
Write-Host "🔐 Trusting HTTPS certificate for development..." -ForegroundColor Yellow
dotnet dev-certs https --trust

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the project
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build

# Run the application
Write-Host "🚀 Starting the API server..." -ForegroundColor Green
Write-Host "📍 API will be available at:" -ForegroundColor Cyan
Write-Host "   HTTP:  http://localhost:5169" -ForegroundColor White
Write-Host "   HTTPS: https://localhost:7049" -ForegroundColor White
Write-Host "   Swagger: http://localhost:5169" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow

dotnet run 