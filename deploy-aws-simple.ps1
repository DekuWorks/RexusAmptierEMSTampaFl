# Simplified AWS Deployment Script for EMS Tampa-FL Amptier
# This script handles the basic deployment process without complex JSON parsing

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

# Build application
function Build-Application {
    Write-Status "Building application..."
    
    # Build the .NET application
    Set-Location RexusOps360.API
    dotnet restore
    dotnet build -c Release
    dotnet publish -c Release -o ./publish
    
    Set-Location ..
    Write-Status "Application built successfully ✓"
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

# Run database migrations
function Run-Migrations {
    Write-Status "Running database migrations..."
    
    # Wait for RDS to be available
    Start-Sleep -Seconds 30
    
    # Get RDS endpoint from CloudFormation outputs
    $outputs = aws cloudformation describe-stacks --stack-name $StackName --region $Region --query 'Stacks[0].Outputs'
    $rdsEndpoint = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "DatabaseEndpoint" }).OutputValue
    
    Write-Status "RDS Endpoint: $rdsEndpoint"
    
    # Run Entity Framework migrations
    Set-Location RexusOps360.API
    dotnet ef database update --connection "Server=$rdsEndpoint;Database=EmsTampaDb;User Id=admin;Password=ProductionEMS2024!;TrustServerCertificate=true;"
    
    Set-Location ..
    Write-Status "Database migrations completed ✓"
}

# Get stack outputs
function Get-StackOutputs {
    $outputs = aws cloudformation describe-stacks --stack-name $StackName --region $Region --query 'Stacks[0].Outputs'
    return $outputs
}

# Main deployment function
function Start-Deployment {
    try {
        Check-Prerequisites
        Build-Application
        Deploy-Infrastructure
        Run-Migrations
        
        Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
        Write-Host "Your EMS Tampa-FL Amptier application is now running on AWS!" -ForegroundColor Cyan
        
        $outputs = Get-StackOutputs
        $albDns = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "LoadBalancerDNS" }).OutputValue
        Write-Host "🌐 Application URL: http://$albDns" -ForegroundColor Yellow
        
        Write-Host ""
        Write-Host "📋 Next Steps:" -ForegroundColor Cyan
        Write-Host "1. Configure your domain name to point to the Load Balancer DNS" -ForegroundColor White
        Write-Host "2. Update SSL certificate for HTTPS" -ForegroundColor White
        Write-Host "3. Set up monitoring and alerts in CloudWatch" -ForegroundColor White
        Write-Host "4. Configure backup and disaster recovery" -ForegroundColor White
        
    }
    catch {
        Write-Error "Deployment failed: $($_.Exception.Message)"
        exit 1
    }
}

# Start the deployment
Start-Deployment 