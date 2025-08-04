# Simple EMS Backend Deployment Script
param(
    [string]$Environment = "production",
    [string]$Region = "us-east-1"
)

$ProjectName = "ems-tampa-amptier"
$StackName = "ems-backend-stack"

Write-Host "🚀 Deploying EMS Backend to AWS ($Environment environment)" -ForegroundColor Green

# Check prerequisites
Write-Host "[INFO] Checking prerequisites..." -ForegroundColor Green

try {
    $awsVersion = aws --version
    Write-Host "[INFO] AWS CLI found: $awsVersion" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] AWS CLI is not installed. Please install it first." -ForegroundColor Red
    exit 1
}

try {
    $caller = aws sts get-caller-identity
    Write-Host "[INFO] AWS credentials configured" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] AWS credentials not configured. Please run 'aws configure' first." -ForegroundColor Red
    Write-Host "[INFO] You can also set environment variables:" -ForegroundColor Yellow
    Write-Host "  `$env:AWS_ACCESS_KEY_ID='your-access-key'" -ForegroundColor Yellow
    Write-Host "  `$env:AWS_SECRET_ACCESS_KEY='your-secret-key'" -ForegroundColor Yellow
    Write-Host "  `$env:AWS_DEFAULT_REGION='$Region'" -ForegroundColor Yellow
    exit 1
}

# Build and publish
Write-Host "[INFO] Building application..." -ForegroundColor Green
dotnet clean
dotnet restore
dotnet build -c Release
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Application built successfully" -ForegroundColor Green

# Deploy using CloudFormation
Write-Host "[INFO] Deploying infrastructure using CloudFormation..." -ForegroundColor Green

try {
    # Check if stack exists
    $stackExists = aws cloudformation describe-stacks --stack-name $StackName 2>$null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[INFO] Updating existing stack..." -ForegroundColor Green
        aws cloudformation update-stack `
            --stack-name $StackName `
            --template-body file://../aws-deploy.yml `
            --capabilities CAPABILITY_IAM `
            --parameters ParameterKey=Environment,ParameterValue=$Environment
    }
    else {
        Write-Host "[INFO] Creating new stack..." -ForegroundColor Green
        aws cloudformation create-stack `
            --stack-name $StackName `
            --template-body file://../aws-deploy.yml `
            --capabilities CAPABILITY_IAM `
            --parameters ParameterKey=Environment,ParameterValue=$Environment
    }
    
    Write-Host "[INFO] CloudFormation deployment initiated" -ForegroundColor Green
    Write-Host "[INFO] Monitor progress with: aws cloudformation describe-stacks --stack-name $StackName" -ForegroundColor Yellow
}
catch {
    Write-Host "[ERROR] CloudFormation deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Alternative: Deploy to Elastic Beanstalk
Write-Host "[INFO] Alternative: Deploy to Elastic Beanstalk" -ForegroundColor Green
Write-Host "[INFO] Commands to run manually:" -ForegroundColor Yellow
Write-Host "1. eb init ems-backend --platform dotnet --region $Region" -ForegroundColor White
Write-Host "2. eb create ems-api-env --instance-type t3.small --single-instance" -ForegroundColor White
Write-Host "3. eb deploy" -ForegroundColor White
Write-Host "4. eb setenv ASPNETCORE_ENVIRONMENT=Production" -ForegroundColor White

Write-Host "[SUCCESS] Deployment script completed!" -ForegroundColor Green
Write-Host "[INFO] Next steps:" -ForegroundColor Yellow
Write-Host "1. Wait for CloudFormation stack to complete" -ForegroundColor White
Write-Host "2. Get your deployment URL from AWS Console" -ForegroundColor White
Write-Host "3. Test the API endpoints" -ForegroundColor White
Write-Host "4. Update frontend to point to new backend URL" -ForegroundColor White 