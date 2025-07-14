# RexusOps360 EMS Deployment Script
# This script tests all APIs and prepares the system for production deployment

param(
    [string]$Environment = "development",
    [string]$ApiUrl = "http://localhost:5000",
    [switch]$SkipTests = $false,
    [switch]$BuildOnly = $false
)

Write-Host "🚀 RexusOps360 EMS Deployment Script" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# Configuration
$Config = @{
    ApiUrl = $ApiUrl
    Environment = $Environment
    TestTimeout = 30
    RetryCount = 3
}

# Test Results
$TestResults = @{
    Total = 0
    Passed = 0
    Failed = 0
    Errors = @()
}

function Write-Status {
    param([string]$Message, [string]$Status = "INFO")
    
    $Color = switch ($Status) {
        "SUCCESS" { "Green" }
        "ERROR" { "Red" }
        "WARNING" { "Yellow" }
        default { "White" }
    }
    
    Write-Host "[$Status] $Message" -ForegroundColor $Color
}

function Test-ApiEndpoint {
    param(
        [string]$Endpoint,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    $TestResults.Total++
    
    try {
        $Uri = "$($Config.ApiUrl)$Endpoint"
        $Params = @{
            Uri = $Uri
            Method = $Method
            TimeoutSec = $Config.TestTimeout
        }
        
        if ($Body) {
            $Params.Body = $Body | ConvertTo-Json -Depth 10
            $Params.ContentType = "application/json"
        }
        
        if ($Headers.Count -gt 0) {
            $Params.Headers = $Headers
        }
        
        $Response = Invoke-RestMethod @Params -ErrorAction Stop
        
        Write-Status "✓ $Method $Endpoint" "SUCCESS"
        $TestResults.Passed++
        return $Response
    }
    catch {
        Write-Status "✗ $Method $Endpoint - $($_.Exception.Message)" "ERROR"
        $TestResults.Failed++
        $TestResults.Errors += "$Method $Endpoint - $($_.Exception.Message)"
        return $null
    }
}

function Test-Authentication {
    Write-Host "`n🔐 Testing Authentication..." -ForegroundColor Cyan
    
    # Test login
    $LoginBody = @{
        username = "admin"
        password = "password123"
    }
    
    $AuthResponse = Test-ApiEndpoint -Endpoint "/api/auth/login" -Method "POST" -Body $LoginBody
    
    if ($AuthResponse) {
        $Global:AuthToken = $AuthResponse.token
        Write-Status "Authentication successful, token obtained" "SUCCESS"
    }
    else {
        Write-Status "Authentication failed" "ERROR"
        return $false
    }
    
    return $true
}

function Test-CoreApis {
    Write-Host "`n📊 Testing Core APIs..." -ForegroundColor Cyan
    
    # Health check
    Test-ApiEndpoint -Endpoint "/api/health"
    
    # Dashboard stats
    Test-ApiEndpoint -Endpoint "/api/dashboard/stats"
    
    # Incidents
    Test-ApiEndpoint -Endpoint "/api/incidents"
    
    # Responders
    Test-ApiEndpoint -Endpoint "/api/responders"
    
    # Equipment
    Test-ApiEndpoint -Endpoint "/api/equipment"
}

function Test-AnalyticsApis {
    Write-Host "`n📈 Testing Analytics APIs..." -ForegroundColor Cyan
    
    # KPI Analytics
    Test-ApiEndpoint -Endpoint "/api/analytics/kpi"
    
    # Incident Heatmap
    Test-ApiEndpoint -Endpoint "/api/analytics/incidents/heatmap"
    
    # Incident Timeline
    Test-ApiEndpoint -Endpoint "/api/analytics/incidents/timeline"
    
    # Responder Performance
    Test-ApiEndpoint -Endpoint "/api/analytics/responders/performance"
    
    # Equipment Analytics
    Test-ApiEndpoint -Endpoint "/api/analytics/equipment/analytics"
}

function Test-NotificationApis {
    Write-Host "`n🔔 Testing Notification APIs..." -ForegroundColor Cyan
    
    # Get notifications
    Test-ApiEndpoint -Endpoint "/api/notifications"
    
    # Create test notification
    $NotificationBody = @{
        title = "Test Notification"
        message = "This is a test notification from deployment script"
        category = "system"
        priority = "normal"
    }
    
    $NotificationResponse = Test-ApiEndpoint -Endpoint "/api/notifications" -Method "POST" -Body $NotificationBody
    
    if ($NotificationResponse) {
        $NotificationId = $NotificationResponse.id
        Test-ApiEndpoint -Endpoint "/api/notifications/$NotificationId"
        Test-ApiEndpoint -Endpoint "/api/notifications/$NotificationId/read" -Method "PUT"
    }
    
    # Test alerts
    Test-ApiEndpoint -Endpoint "/api/notifications/alerts"
    Test-ApiEndpoint -Endpoint "/api/notifications/stats"
}

function Test-LocationApis {
    Write-Host "`n📍 Testing Location APIs..." -ForegroundColor Cyan
    
    # Responder locations
    Test-ApiEndpoint -Endpoint "/api/location/responders"
    
    # Update responder location
    $LocationBody = @{
        coordinates = @{
            latitude = 27.9506
            longitude = -82.4572
        }
        address = "Downtown Tampa"
        speed = 0
        heading = 0
        accuracy = 10
    }
    
    Test-ApiEndpoint -Endpoint "/api/location/responders/1/location" -Method "POST" -Body $LocationBody
    
    # Nearby incidents
    Test-ApiEndpoint -Endpoint "/api/location/incidents/nearby?lat=27.9506&lng=-82.4572&radiusKm=5"
    
    # Geo-fences
    Test-ApiEndpoint -Endpoint "/api/location/geo-fences"
    
    # Optimized routes
    Test-ApiEndpoint -Endpoint "/api/location/optimization/routes?lat=27.9506&lng=-82.4572"
}

function Test-EquipmentManagementApis {
    Write-Host "`n🛠️ Testing Equipment Management APIs..." -ForegroundColor Cyan
    
    # Inventory status
    Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/inventory"
    
    # Maintenance schedules
    Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/maintenance/schedule"
    
    # Barcode scans
    Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/barcode/scans"
    
    # Equipment alerts
    Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/alerts"
    
    # Usage analytics
    Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/analytics/usage"
    
    # Create test maintenance schedule
    $MaintenanceBody = @{
        equipmentId = 1
        maintenanceType = "Preventive"
        description = "Routine maintenance check"
        scheduledDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ")
        estimatedDuration = 2
        assignedTechnician = "John Tech"
        priority = "normal"
    }
    
    $MaintenanceResponse = Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/maintenance/schedule" -Method "POST" -Body $MaintenanceBody
    
    if ($MaintenanceResponse) {
        $MaintenanceId = $MaintenanceResponse.id
        Test-ApiEndpoint -Endpoint "/api/equipmentmanagement/maintenance/schedule/$MaintenanceId"
    }
}

function Test-Performance {
    Write-Host "`n⚡ Testing Performance..." -ForegroundColor Cyan
    
    $Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    # Test concurrent requests
    $Jobs = @()
    for ($i = 1; $i -le 10; $i++) {
        $Jobs += Start-Job -ScriptBlock {
            param($ApiUrl)
            try {
                $Response = Invoke-RestMethod -Uri "$ApiUrl/api/health" -TimeoutSec 10
                return "SUCCESS"
            }
            catch {
                return "FAILED"
            }
        } -ArgumentList $Config.ApiUrl
    }
    
    $Results = $Jobs | Wait-Job | Receive-Job
    $Jobs | Remove-Job
    
    $SuccessCount = ($Results | Where-Object { $_ -eq "SUCCESS" }).Count
    $FailureCount = ($Results | Where-Object { $_ -eq "FAILED" }).Count
    
    $Stopwatch.Stop()
    $ElapsedTime = $Stopwatch.Elapsed.TotalSeconds
    
    Write-Status "Performance test completed in $($ElapsedTime.ToString('F2'))s" "SUCCESS"
    Write-Status "Concurrent requests: $SuccessCount successful, $FailureCount failed" "INFO"
    
    if ($SuccessCount -eq 10) {
        Write-Status "All concurrent requests successful" "SUCCESS"
    }
    else {
        Write-Status "Some concurrent requests failed" "WARNING"
    }
}

function Test-Security {
    Write-Host "`n🔒 Testing Security..." -ForegroundColor Cyan
    
    # Test unauthorized access
    $UnauthorizedResponse = Test-ApiEndpoint -Endpoint "/api/incidents" -Headers @{}
    
    if (-not $UnauthorizedResponse) {
        Write-Status "Security: Unauthorized access properly blocked" "SUCCESS"
    }
    else {
        Write-Status "Security: Unauthorized access not properly blocked" "ERROR"
    }
    
    # Test with valid token
    if ($Global:AuthToken) {
        $AuthorizedResponse = Test-ApiEndpoint -Endpoint "/api/incidents" -Headers @{
            "Authorization" = "Bearer $Global:AuthToken"
        }
        
        if ($AuthorizedResponse) {
            Write-Status "Security: Authorized access working" "SUCCESS"
        }
        else {
            Write-Status "Security: Authorized access failed" "ERROR"
        }
    }
}

function Show-TestResults {
    Write-Host "`n📋 Test Results Summary" -ForegroundColor Green
    Write-Host "=======================" -ForegroundColor Green
    
    Write-Host "Total Tests: $($TestResults.Total)" -ForegroundColor White
    Write-Host "Passed: $($TestResults.Passed)" -ForegroundColor Green
    Write-Host "Failed: $($TestResults.Failed)" -ForegroundColor Red
    
    $SuccessRate = if ($TestResults.Total -gt 0) { 
        [math]::Round(($TestResults.Passed / $TestResults.Total) * 100, 2) 
    } else { 0 }
    
    Write-Host "Success Rate: $SuccessRate%" -ForegroundColor $(if ($SuccessRate -ge 90) { "Green" } elseif ($SuccessRate -ge 70) { "Yellow" } else { "Red" })
    
    if ($TestResults.Errors.Count -gt 0) {
        Write-Host "`n❌ Errors:" -ForegroundColor Red
        $TestResults.Errors | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor Red
        }
    }
    
    if ($SuccessRate -ge 90) {
        Write-Host "`n✅ Deployment ready!" -ForegroundColor Green
        return $true
    }
    else {
        Write-Host "`n❌ Deployment failed - fix errors before proceeding" -ForegroundColor Red
        return $false
    }
}

function Build-Project {
    Write-Host "`n🔨 Building Project..." -ForegroundColor Cyan
    
    try {
        Set-Location "RexusOps360.API"
        
        # Restore packages
        Write-Status "Restoring NuGet packages..."
        dotnet restore
        
        # Build project
        Write-Status "Building project..."
        dotnet build --configuration Release
        
        # Run tests if not skipped
        if (-not $SkipTests) {
            Write-Status "Running unit tests..."
            dotnet test --no-build
        }
        
        Set-Location ".."
        Write-Status "Build completed successfully" "SUCCESS"
        return $true
    }
    catch {
        Write-Status "Build failed: $($_.Exception.Message)" "ERROR"
        Set-Location ".."
        return $false
    }
}

function Start-Application {
    Write-Host "`n🚀 Starting Application..." -ForegroundColor Cyan
    
    try {
        Set-Location "RexusOps360.API"
        
        # Start the application in background
        $Process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -WindowStyle Hidden
        
        # Wait for application to start
        Write-Status "Waiting for application to start..."
        Start-Sleep -Seconds 10
        
        # Test if application is responding
        $HealthCheck = Test-ApiEndpoint -Endpoint "/api/health"
        
        if ($HealthCheck) {
            Write-Status "Application started successfully" "SUCCESS"
            return $Process
        }
        else {
            Write-Status "Application failed to start" "ERROR"
            Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
            return $null
        }
    }
    catch {
        Write-Status "Failed to start application: $($_.Exception.Message)" "ERROR"
        Set-Location ".."
        return $null
    }
    finally {
        Set-Location ".."
    }
}

function Stop-Application {
    param([object]$Process)
    
    if ($Process) {
        Write-Status "Stopping application..."
        Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
        Write-Status "Application stopped" "SUCCESS"
    }
}

function Create-DeploymentPackage {
    Write-Host "`n📦 Creating Deployment Package..." -ForegroundColor Cyan
    
    $DeploymentDir = "deployment"
    $Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $PackageName = "RexusOps360-EMS-$Timestamp"
    
    # Create deployment directory
    if (Test-Path $DeploymentDir) {
        Remove-Item $DeploymentDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $DeploymentDir | Out-Null
    
    # Copy necessary files
    Write-Status "Copying application files..."
    Copy-Item "RexusOps360.API" -Destination "$DeploymentDir/RexusOps360.API" -Recurse
    Copy-Item "frontend" -Destination "$DeploymentDir/frontend" -Recurse
    Copy-Item "README.md" -Destination "$DeploymentDir/"
    Copy-Item "requirements.txt" -Destination "$DeploymentDir/"
    
    # Create deployment script
    $DeployScript = @"
# RexusOps360 EMS Deployment Script
# Generated on $(Get-Date)

Write-Host "Deploying RexusOps360 EMS..." -ForegroundColor Green

# Install .NET if not present
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Installing .NET..." -ForegroundColor Yellow
    # Add .NET installation logic here
}

# Build and run application
Set-Location "RexusOps360.API"
dotnet restore
dotnet build --configuration Release
dotnet run --environment Production

Write-Host "Deployment completed!" -ForegroundColor Green
"@
    
    $DeployScript | Out-File -FilePath "$DeploymentDir/deploy.ps1" -Encoding UTF8
    
    # Create Docker files
    $Dockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RexusOps360.API/RexusOps360.API.csproj", "RexusOps360.API/"]
RUN dotnet restore "RexusOps360.API/RexusOps360.API.csproj"
COPY . .
WORKDIR "/src/RexusOps360.API"
RUN dotnet build "RexusOps360.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RexusOps360.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RexusOps360.API.dll"]
"@
    
    $Dockerfile | Out-File -FilePath "$DeploymentDir/Dockerfile" -Encoding UTF8
    
    # Create docker-compose file
    $DockerCompose = @"
version: '3.8'
services:
  rexusops360-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
"@
    
    $DockerCompose | Out-File -FilePath "$DeploymentDir/docker-compose.yml" -Encoding UTF8
    
    # Create package
    Compress-Archive -Path $DeploymentDir -DestinationPath "$PackageName.zip" -Force
    
    Write-Status "Deployment package created: $PackageName.zip" "SUCCESS"
    return "$PackageName.zip"
}

# Main execution
try {
    Write-Host "Environment: $($Config.Environment)" -ForegroundColor Yellow
    Write-Host "API URL: $($Config.ApiUrl)" -ForegroundColor Yellow
    
    if (-not $BuildOnly) {
        # Build project
        if (-not (Build-Project)) {
            exit 1
        }
        
        # Start application
        $AppProcess = Start-Application
        if (-not $AppProcess) {
            exit 1
        }
        
        # Wait for application to be ready
        Start-Sleep -Seconds 5
        
        # Run tests
        if (-not $SkipTests) {
            Test-Authentication
            Test-CoreApis
            Test-AnalyticsApis
            Test-NotificationApis
            Test-LocationApis
            Test-EquipmentManagementApis
            Test-Performance
            Test-Security
            
            $TestSuccess = Show-TestResults
            
            if (-not $TestSuccess) {
                Stop-Application -Process $AppProcess
                exit 1
            }
        }
        
        # Create deployment package
        $PackagePath = Create-DeploymentPackage
        
        Write-Host "`n🎉 Deployment completed successfully!" -ForegroundColor Green
        Write-Host "Package: $PackagePath" -ForegroundColor Green
        Write-Host "API URL: $($Config.ApiUrl)" -ForegroundColor Green
        Write-Host "Swagger UI: $($Config.ApiUrl)" -ForegroundColor Green
        Write-Host "Advanced Dashboard: $($Config.ApiUrl)/frontend/advanced-dashboard.html" -ForegroundColor Green
        
        # Keep application running if not in build-only mode
        if (-not $BuildOnly) {
            Write-Host "`nPress Ctrl+C to stop the application..." -ForegroundColor Yellow
            try {
                while ($true) {
                    Start-Sleep -Seconds 1
                }
            }
            finally {
                Stop-Application -Process $AppProcess
            }
        }
        else {
            Stop-Application -Process $AppProcess
        }
    }
    else {
        # Build only mode
        Build-Project
        Create-DeploymentPackage
    }
}
catch {
    Write-Status "Deployment failed: $($_.Exception.Message)" "ERROR"
    exit 1
} 