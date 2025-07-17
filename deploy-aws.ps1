# PowerShell AWS Deployment Script for EMS Tampa-FL Amptier
# Enhanced version with clustering, hotspot detection, and system integration features

param(
    [string]$Environment = "production"
)

# Configuration
$ProjectName = "ems-tampa-amptier"
$Region = "us-east-1"
$StackName = "ems-tampa-stack"

Write-Host "🚀 Deploying EMS Tampa-FL Amptier to AWS ($Environment environment)" -ForegroundColor Green

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check prerequisites
function Check-Prerequisites {
    Write-Status "Checking prerequisites..."
    
    try {
        $awsVersion = aws --version
        Write-Status "AWS CLI found: $awsVersion"
    }
    catch {
        Write-Error "AWS CLI is not installed. Please install it first."
        exit 1
    }
    
    try {
        $dockerVersion = docker --version
        Write-Status "Docker found: $dockerVersion"
    }
    catch {
        Write-Error "Docker is not installed. Please install it first."
        exit 1
    }
    
    try {
        $caller = aws sts get-caller-identity
        Write-Status "AWS credentials configured ✓"
    }
    catch {
        Write-Error "AWS credentials not configured. Please run 'aws configure' first."
        exit 1
    }
    
    Write-Status "Prerequisites check passed ✓"
}

# Build and package application
function Build-Application {
    Write-Status "Building application..."
    
    # Build the .NET application
    Set-Location RexusOps360.API
    dotnet restore
    dotnet build -c Release
    dotnet publish -c Release -o ./publish
    
    # Create Docker image
    docker build -t $ProjectName:$Environment .
    
    Set-Location ..
    Write-Status "Application built successfully ✓"
}

# Create ECR repository and push image
function Setup-ECR {
    Write-Status "Setting up ECR repository..."
    
    # Create ECR repository if it doesn't exist
    try {
        aws ecr describe-repositories --repository-names $ProjectName --region $Region 2>$null
    }
    catch {
        aws ecr create-repository --repository-name $ProjectName --region $Region
    }
    
    # Get ECR login token
    $accountId = aws sts get-caller-identity --query Account --output text
    aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $accountId.dkr.ecr.$Region.amazonaws.com
    
    # Tag and push image
    $ecrRepo = "$accountId.dkr.ecr.$Region.amazonaws.com/$ProjectName"
    docker tag $ProjectName:$Environment $ecrRepo:$Environment
    docker push $ecrRepo:$Environment
    
    Write-Status "ECR setup completed ✓"
}

# Deploy infrastructure with CloudFormation
function Deploy-Infrastructure {
    Write-Status "Deploying infrastructure with CloudFormation..."
    
    # Deploy the CloudFormation stack
    aws cloudformation deploy `
        --template-file aws-deploy.yml `
        --stack-name $StackName `
        --parameter-overrides Environment=$Environment ProjectName=$ProjectName `
        --capabilities CAPABILITY_NAMED_IAM `
        --region $Region
    
    # Wait for stack to complete
    aws cloudformation wait stack-create-complete --stack-name $StackName --region $Region
    
    Write-Status "Infrastructure deployed successfully ✓"
}

# Get stack outputs
function Get-StackOutputs {
    $outputs = aws cloudformation describe-stacks --stack-name $StackName --region $Region --query 'Stacks[0].Outputs'
    return $outputs
}

# Configure application settings
function Configure-Application {
    Write-Status "Configuring application settings..."
    
    $outputs = Get-StackOutputs
    $rdsEndpoint = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "RdsEndpoint" }).OutputValue
    $albDns = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "LoadBalancerDNS" }).OutputValue
    
    Write-Status "RDS Endpoint: $rdsEndpoint"
    Write-Status "Load Balancer DNS: $albDns"
    
    # Update application configuration
    $configContent = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=$rdsEndpoint;Database=EmsTampaDb;User Id=emsuser;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "YourSuperSecretKeyHere12345678901234567890",
    "Issuer": "RexusOps360",
    "Audience": "RexusOps360Users",
    "ExpiryHours": 8
  },
  "SystemIntegrations": {
    "ScadaEndpoint": "https://scada.rexusops360.com/api",
    "GpsEndpoint": "https://gps.rexusops360.com/api",
    "WeatherApiKey": "your_weather_api_key_here",
    "GisEndpoint": "https://gis.rexusops360.com/api"
  },
  "HotspotDetection": {
    "Threshold": 3,
    "TimeWindowMinutes": 120,
    "RadiusKm": 1.0
  },
  "Clustering": {
    "MaxDistanceKm": 1.0,
    "MaxTimeWindowMinutes": 30,
    "MinIncidentsForCluster": 2
  }
}
"@
    
    $configContent | Out-File -FilePath "RexusOps360.API/appsettings.Production.json" -Encoding UTF8
    
    Write-Status "Application configuration updated ✓"
}

# Run database migrations
function Run-Migrations {
    Write-Status "Running database migrations..."
    
    # Wait for RDS to be available
    Start-Sleep -Seconds 30
    
    $outputs = Get-StackOutputs
    $rdsEndpoint = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "RdsEndpoint" }).OutputValue
    
    # Run Entity Framework migrations
    Set-Location RexusOps360.API
    dotnet ef database update --connection "Server=$rdsEndpoint;Database=EmsTampaDb;User Id=emsuser;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
    
    Set-Location ..
    Write-Status "Database migrations completed ✓"
}

# Deploy application to ECS
function Deploy-Application {
    Write-Status "Deploying application to ECS..."
    
    # Update ECS service
    aws ecs update-service `
        --cluster $ProjectName-cluster `
        --service $ProjectName-service `
        --force-new-deployment `
        --region $Region
    
    # Wait for deployment to complete
    aws ecs wait services-stable `
        --cluster $ProjectName-cluster `
        --services $ProjectName-service `
        --region $Region
    
    Write-Status "Application deployed successfully ✓"
}

# Main deployment function
function Start-Deployment {
    try {
        Check-Prerequisites
        Build-Application
        Setup-ECR
        Deploy-Infrastructure
        Configure-Application
        Run-Migrations
        Deploy-Application
        
        Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
        Write-Host "Your EMS Tampa-FL Amptier application is now running on AWS!" -ForegroundColor Cyan
        
        $outputs = Get-StackOutputs
        $albDns = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "LoadBalancerDNS" }).OutputValue
        Write-Host "🌐 Application URL: http://$albDns" -ForegroundColor Yellow
        
    }
    catch {
        Write-Error "Deployment failed: $($_.Exception.Message)"
        exit 1
    }
}

# Start the deployment
Start-Deployment 