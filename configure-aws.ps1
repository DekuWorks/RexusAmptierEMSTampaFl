# AWS Configuration Helper Script
Write-Host "🔐 AWS Configuration Helper" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

Write-Host ""
Write-Host "Choose your AWS configuration method:" -ForegroundColor Yellow
Write-Host "1. Interactive configuration (aws configure)" -ForegroundColor White
Write-Host "2. Environment variables" -ForegroundColor White
Write-Host "3. Manual configuration" -ForegroundColor White
Write-Host "4. Check current configuration" -ForegroundColor White

$choice = Read-Host "Enter your choice (1-4)"

switch ($choice) {
    "1" {
        Write-Host "Running interactive AWS configuration..." -ForegroundColor Green
        aws configure
    }
    "2" {
        Write-Host "Setting up environment variables..." -ForegroundColor Green
        $accessKey = Read-Host "Enter your AWS Access Key ID"
        $secretKey = Read-Host "Enter your AWS Secret Access Key" -AsSecureString
        $region = Read-Host "Enter your AWS Region (e.g., us-east-1)"
        
        $env:AWS_ACCESS_KEY_ID = $accessKey
        $env:AWS_SECRET_ACCESS_KEY = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secretKey))
        $env:AWS_DEFAULT_REGION = $region
        
        Write-Host "Environment variables set for this session" -ForegroundColor Green
    }
    "3" {
        Write-Host "Manual configuration instructions:" -ForegroundColor Yellow
        Write-Host "1. Create AWS credentials file at: ~/.aws/credentials" -ForegroundColor White
        Write-Host "2. Add your credentials:" -ForegroundColor White
        Write-Host "   [default]" -ForegroundColor Cyan
        Write-Host "   aws_access_key_id = YOUR_ACCESS_KEY" -ForegroundColor Cyan
        Write-Host "   aws_secret_access_key = YOUR_SECRET_KEY" -ForegroundColor Cyan
        Write-Host "3. Create AWS config file at: ~/.aws/config" -ForegroundColor White
        Write-Host "4. Add your region:" -ForegroundColor White
        Write-Host "   [default]" -ForegroundColor Cyan
        Write-Host "   region = us-east-1" -ForegroundColor Cyan
        Write-Host "   output = json" -ForegroundColor Cyan
    }
    "4" {
        Write-Host "Checking current AWS configuration..." -ForegroundColor Green
        try {
            $identity = aws sts get-caller-identity
            Write-Host "✅ AWS credentials configured successfully!" -ForegroundColor Green
            Write-Host "Account: $($identity | ConvertFrom-Json | Select-Object -ExpandProperty Account)" -ForegroundColor White
            Write-Host "User: $($identity | ConvertFrom-Json | Select-Object -ExpandProperty Arn)" -ForegroundColor White
        }
        catch {
            Write-Host "❌ AWS credentials not configured or invalid" -ForegroundColor Red
            Write-Host "Please configure your AWS credentials first." -ForegroundColor Yellow
        }
    }
    default {
        Write-Host "Invalid choice. Please run the script again." -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Verify configuration: aws sts get-caller-identity" -ForegroundColor White
Write-Host "2. Run deployment: .\RexusOps360.API\deploy-simple.ps1" -ForegroundColor White
Write-Host "3. Check deployment guide: AWS_DEPLOYMENT_STEPS.md" -ForegroundColor White 