#!/bin/bash

# Enhanced AWS Deployment Script for EMS Tampa-FL Amptier
# Includes clustering, hotspot detection, and system integration features

set -e

# Configuration
PROJECT_NAME="ems-tampa-amptier"
REGION="us-east-1"
STACK_NAME="ems-tampa-stack"
ENVIRONMENT=${1:-production}

echo "🚀 Deploying EMS Tampa-FL Amptier to AWS ($ENVIRONMENT environment)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    if ! command -v aws &> /dev/null; then
        print_error "AWS CLI is not installed. Please install it first."
        exit 1
    fi
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install it first."
        exit 1
    fi
    
    if ! aws sts get-caller-identity &> /dev/null; then
        print_error "AWS credentials not configured. Please run 'aws configure' first."
        exit 1
    fi
    
    print_status "Prerequisites check passed ✓"
}

# Build and package application
build_application() {
    print_status "Building application..."
    
    # Build the .NET application
    cd RexusOps360.API
    dotnet restore
    dotnet build -c Release
    dotnet publish -c Release -o ./publish
    
    # Create Docker image
    docker build -t $PROJECT_NAME:$ENVIRONMENT .
    
    print_status "Application built successfully ✓"
}

# Create ECR repository and push image
setup_ecr() {
    print_status "Setting up ECR repository..."
    
    # Create ECR repository if it doesn't exist
    aws ecr describe-repositories --repository-names $PROJECT_NAME --region $REGION 2>/dev/null || \
    aws ecr create-repository --repository-name $PROJECT_NAME --region $REGION
    
    # Get ECR login token
    aws ecr get-login-password --region $REGION | docker login --username AWS --password-stdin \
        $(aws sts get-caller-identity --query Account --output text).dkr.ecr.$REGION.amazonaws.com
    
    # Tag and push image
    ECR_REPO=$(aws sts get-caller-identity --query Account --output text).dkr.ecr.$REGION.amazonaws.com/$PROJECT_NAME
    docker tag $PROJECT_NAME:$ENVIRONMENT $ECR_REPO:$ENVIRONMENT
    docker push $ECR_REPO:$ENVIRONMENT
    
    print_status "ECR setup completed ✓"
}

# Deploy infrastructure with CloudFormation
deploy_infrastructure() {
    print_status "Deploying infrastructure with CloudFormation..."
    
    # Deploy the CloudFormation stack
    aws cloudformation deploy \
        --template-file aws-deploy.yml \
        --stack-name $STACK_NAME \
        --parameter-overrides \
            Environment=$ENVIRONMENT \
            ProjectName=$PROJECT_NAME \
        --capabilities CAPABILITY_NAMED_IAM \
        --region $REGION
    
    # Wait for stack to complete
    aws cloudformation wait stack-create-complete \
        --stack-name $STACK_NAME \
        --region $REGION
    
    print_status "Infrastructure deployed successfully ✓"
}

# Configure application settings
configure_application() {
    print_status "Configuring application settings..."
    
    # Get stack outputs
    STACK_OUTPUTS=$(aws cloudformation describe-stacks \
        --stack-name $STACK_NAME \
        --region $REGION \
        --query 'Stacks[0].Outputs')
    
    # Extract values
    RDS_ENDPOINT=$(echo $STACK_OUTPUTS | jq -r '.[] | select(.OutputKey=="RdsEndpoint").OutputValue')
    ALB_DNS=$(echo $STACK_OUTPUTS | jq -r '.[] | select(.OutputKey=="LoadBalancerDNS").OutputValue')
    
    print_status "RDS Endpoint: $RDS_ENDPOINT"
    print_status "Load Balancer DNS: $ALB_DNS"
    
    # Update application configuration
    cat > RexusOps360.API/appsettings.Production.json << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=$RDS_ENDPOINT;Database=EmsTampaDb;User Id=emsuser;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
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
EOF
    
    print_status "Application configuration updated ✓"
}

# Run database migrations
run_migrations() {
    print_status "Running database migrations..."
    
    # Wait for RDS to be available
    sleep 30
    
    # Run Entity Framework migrations
    cd RexusOps360.API
    dotnet ef database update --connection "Server=$RDS_ENDPOINT;Database=EmsTampaDb;User Id=emsuser;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
    
    print_status "Database migrations completed ✓"
}

# Deploy application to ECS
deploy_application() {
    print_status "Deploying application to ECS..."
    
    # Update ECS service
    aws ecs update-service \
        --cluster $PROJECT_NAME-cluster \
        --service $PROJECT_NAME-service \
        --force-new-deployment \
        --region $REGION
    
    # Wait for deployment to complete
    aws ecs wait services-stable \
        --cluster $PROJECT_NAME-cluster \
        --services $PROJECT_NAME-service \
        --region $REGION
    
    print_status "Application deployed successfully ✓"
}

# Run health checks
health_check() {
    print_status "Running health checks..."
    
    # Get ALB DNS name
    ALB_DNS=$(aws cloudformation describe-stacks \
        --stack-name $STACK_NAME \
        --region $REGION \
        --query 'Stacks[0].Outputs[?OutputKey==`LoadBalancerDNS`].OutputValue' \
        --output text)
    
    # Wait for application to be ready
    sleep 60
    
    # Test health endpoint
    if curl -f http://$ALB_DNS/health; then
        print_status "Health check passed ✓"
    else
        print_error "Health check failed"
        exit 1
    fi
    
    # Test API endpoints
    if curl -f http://$ALB_DNS/api/incidents; then
        print_status "API endpoints working ✓"
    else
        print_error "API endpoints failed"
        exit 1
    fi
}

# Setup monitoring and alerts
setup_monitoring() {
    print_status "Setting up monitoring and alerts..."
    
    # Create CloudWatch dashboard
    aws cloudwatch put-dashboard \
        --dashboard-name "$PROJECT_NAME-dashboard" \
        --dashboard-body file://monitoring-dashboard.json \
        --region $REGION
    
    # Create alarms for key metrics
    aws cloudwatch put-metric-alarm \
        --alarm-name "$PROJECT_NAME-cpu-high" \
        --alarm-description "High CPU utilization" \
        --metric-name CPUUtilization \
        --namespace AWS/ECS \
        --statistic Average \
        --period 300 \
        --threshold 80 \
        --comparison-operator GreaterThanThreshold \
        --evaluation-periods 2 \
        --region $REGION
    
    print_status "Monitoring setup completed ✓"
}

# Main deployment function
main() {
    print_status "Starting deployment process..."
    
    check_prerequisites
    build_application
    setup_ecr
    deploy_infrastructure
    configure_application
    run_migrations
    deploy_application
    health_check
    setup_monitoring
    
    print_status "🎉 Deployment completed successfully!"
    print_status "Application URL: http://$(aws cloudformation describe-stacks --stack-name $STACK_NAME --region $REGION --query 'Stacks[0].Outputs[?OutputKey==`LoadBalancerDNS`].OutputValue' --output text)"
    print_status "Dashboard URL: https://console.aws.amazon.com/cloudwatch/home?region=$REGION#dashboards:name=$PROJECT_NAME-dashboard"
}

# Run main function
main "$@" 