# PowerShell Script to Deploy EMS Backend to AWS
# Alternative to EB CLI that works reliably on Windows

param(
    [string]$Environment = "production",
    [string]$Region = "us-east-1"
)

# Configuration
$ProjectName = "ems-tampa-amptier"
$StackName = "ems-backend-stack"

Write-Host "🚀 Deploying EMS Backend to AWS ($Environment environment)" -ForegroundColor Green

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
    
    # Clean previous builds
    dotnet clean
    
    # Publish the application
    dotnet publish -c Release -o ./publish
    
    Write-Status "Application built successfully ✓"
}

# Create deployment package
function Create-DeploymentPackage {
    Write-Status "Creating deployment package..."
    
    # Create deployment directory
    if (Test-Path "./deploy") {
        Remove-Item "./deploy" -Recurse -Force
    }
    New-Item -ItemType Directory -Path "./deploy"
    
    # Copy published files
    Copy-Item "./publish/*" -Destination "./deploy" -Recurse
    
    # Copy additional files needed for deployment
    Copy-Item "./appsettings.json" -Destination "./deploy"
    Copy-Item "./appsettings.Production.json" -Destination "./deploy" -ErrorAction SilentlyContinue
    
    # Create deployment zip
    Compress-Archive -Path "./deploy/*" -DestinationPath "./ems-backend-deploy.zip" -Force
    
    Write-Status "Deployment package created ✓"
}

# Upload to S3
function Upload-ToS3 {
    Write-Status "Uploading to S3..."
    
    $bucketName = "$ProjectName-deployments-$Region"
    
    # Create S3 bucket if it doesn't exist
    try {
        aws s3api head-bucket --bucket $bucketName --region $Region 2>$null
    }
    catch {
        aws s3 mb s3://$bucketName --region $Region
    }
    
    # Upload deployment package
    aws s3 cp "./ems-backend-deploy.zip" "s3://$bucketName/ems-backend-deploy.zip" --region $Region
    
    Write-Status "Uploaded to S3: s3://$bucketName/ems-backend-deploy.zip"
}

# Deploy infrastructure with CloudFormation
function Deploy-Infrastructure {
    Write-Status "Deploying infrastructure with CloudFormation..."
    
    # Deploy the CloudFormation stack
    aws cloudformation deploy `
        --template-file "../aws-deploy.yml" `
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

# Run database migrations
function Run-Migrations {
    Write-Status "Running database migrations..."
    
    # Wait for RDS to be available
    Start-Sleep -Seconds 30
    
    $outputs = Get-StackOutputs
    $rdsEndpoint = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "DatabaseEndpoint" }).OutputValue
    
    Write-Status "RDS Endpoint: $rdsEndpoint"
    
    # Run Entity Framework migrations
    dotnet ef database update --connection "Server=$rdsEndpoint;Database=EmsTampaDb;User Id=admin;Password=ProductionEMS2024!;TrustServerCertificate=true;"
    
    Write-Status "Database migrations completed"
}

# Main deployment function
function Start-Deployment {
    try {
        Check-Prerequisites
        Build-Application
        Create-DeploymentPackage
        Upload-ToS3
        Deploy-Infrastructure
        Run-Migrations
        
        Write-Host "🎉 Backend deployment completed successfully!" -ForegroundColor Green
        Write-Host "Your EMS Backend is now running on AWS!" -ForegroundColor Cyan
        
        $outputs = Get-StackOutputs
        $albDns = ($outputs | ConvertFrom-Json | Where-Object { $_.OutputKey -eq "LoadBalancerDNS" }).OutputValue
        Write-Host "🌐 Backend URL: http://$albDns" -ForegroundColor Yellow
        Write-Host "🔍 Health Check: http://$albDns/health" -ForegroundColor Yellow
        Write-Host "📚 API Docs: http://$albDns/swagger" -ForegroundColor Yellow
        
        Write-Host ""
        Write-Host "📋 Next Steps:" -ForegroundColor Cyan
        Write-Host "1. Test the API endpoints" -ForegroundColor White
        Write-Host "2. Update frontend to point to the new backend URL" -ForegroundColor White
        Write-Host "3. Configure SSL certificate for HTTPS" -ForegroundColor White
        Write-Host "4. Set up monitoring and alerts" -ForegroundColor White
        
    }
    catch {
        Write-Error "Deployment failed: $($_.Exception.Message)"
        exit 1
    }
}

# Start the deployment
Start-Deployment 