# RexusOps360 C# Backend Startup Script
# This script starts the C# ASP.NET Core backend

Write-Host "🚀 Starting RexusOps360 C# Backend..." -ForegroundColor Green

# Navigate to the API directory
Set-Location "RexusOps360.API"

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 6.0 SDK" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the project
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build

# Run the application
Write-Host "🚀 Starting application..." -ForegroundColor Green
Write-Host "📍 Application will be available at:" -ForegroundColor Cyan
Write-Host "   - Main App: http://localhost:5169" -ForegroundColor White
Write-Host "   - API Docs: http://localhost:5169/swagger" -ForegroundColor White
Write-Host "   - Login: http://localhost:5169/web/login" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow

dotnet run 