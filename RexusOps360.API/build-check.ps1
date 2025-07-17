# Build Check Script for RexusOps360.API
Write-Host "=== RexusOps360.API Build Check ===" -ForegroundColor Green

Write-Host "`n1. Checking project structure..." -ForegroundColor Yellow
if (Test-Path "Models") { Write-Host "✓ Models directory exists" -ForegroundColor Green } else { Write-Host "✗ Models directory missing" -ForegroundColor Red }
if (Test-Path "Controllers") { Write-Host "✓ Controllers directory exists" -ForegroundColor Green } else { Write-Host "✗ Controllers directory missing" -ForegroundColor Red }
if (Test-Path "Services") { Write-Host "✓ Services directory exists" -ForegroundColor Green } else { Write-Host "✗ Services directory missing" -ForegroundColor Red }
if (Test-Path "Data") { Write-Host "✓ Data directory exists" -ForegroundColor Green } else { Write-Host "✗ Data directory missing" -ForegroundColor Red }

Write-Host "`n2. Checking required files..." -ForegroundColor Yellow
$requiredFiles = @(
    "Models/Incident.cs",
    "Models/User.cs", 
    "Models/Equipment.cs",
    "Models/Responder.cs",
    "Models/AuditLog.cs",
    "Models/SystemIntegration.cs",
    "Models/Hotspot.cs",
    "Controllers/IncidentsController.cs",
    "Controllers/SystemIntegrationController.cs",
    "Controllers/HotspotController.cs",
    "Services/IncidentClusteringService.cs",
    "Services/HotspotDetectionService.cs",
    "Services/SystemIntegrationService.cs",
    "Data/EmsDbContext.cs",
    "Program.cs",
    "RexusOps360.API.csproj"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file missing" -ForegroundColor Red
    }
}

Write-Host "`n3. Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "`n4. Building project..." -ForegroundColor Yellow
dotnet build --verbosity normal

Write-Host "`n=== Build Check Complete ===" -ForegroundColor Green 