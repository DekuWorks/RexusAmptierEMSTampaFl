#!/bin/bash

# EMS Tampa-FL Amptier AWS Deployment Script
# This script deploys the EMS system to AWS using CloudFormation

set -e

# Configuration
STACK_NAME="ems-tampa-production"
REGION="us-east-1"
ENVIRONMENT="Production"
INSTANCE_TYPE="t3.medium"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Starting EMS Tampa-FL Amptier AWS Deployment${NC}"

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo -e "${RED}❌ AWS CLI is not installed. Please install it first.${NC}"
    exit 1
fi

# Check if AWS credentials are configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo -e "${RED}❌ AWS credentials not configured. Please run 'aws configure' first.${NC}"
    exit 1
fi

echo -e "${YELLOW}📋 Prerequisites check passed${NC}"

# Create S3 bucket for deployment artifacts
BUCKET_NAME="ems-tampa-deployment-$(date +%s)"
echo -e "${YELLOW}📦 Creating S3 bucket: $BUCKET_NAME${NC}"
aws s3 mb s3://$BUCKET_NAME --region $REGION

# Upload application files to S3
echo -e "${YELLOW}📤 Uploading application files to S3${NC}"
tar -czf ems-app.tar.gz -C . .
aws s3 cp ems-app.tar.gz s3://$BUCKET_NAME/
aws s3 cp docker-compose.yml s3://$BUCKET_NAME/
aws s3 cp Dockerfile s3://$BUCKET_NAME/

# Deploy CloudFormation stack
echo -e "${YELLOW}🏗️  Deploying CloudFormation stack: $STACK_NAME${NC}"
aws cloudformation deploy \
    --template-file aws-deploy.yml \
    --stack-name $STACK_NAME \
    --parameter-overrides \
        Environment=$ENVIRONMENT \
        InstanceType=$INSTANCE_TYPE \
    --capabilities CAPABILITY_IAM \
    --region $REGION

# Wait for stack to complete
echo -e "${YELLOW}⏳ Waiting for stack deployment to complete...${NC}"
aws cloudformation wait stack-create-complete \
    --stack-name $STACK_NAME \
    --region $REGION

# Get stack outputs
echo -e "${YELLOW}📊 Getting deployment outputs${NC}"
LOAD_BALANCER_DNS=$(aws cloudformation describe-stacks \
    --stack-name $STACK_NAME \
    --region $REGION \
    --query 'Stacks[0].Outputs[?OutputKey==`LoadBalancerDNS`].OutputValue' \
    --output text)

DATABASE_ENDPOINT=$(aws cloudformation describe-stacks \
    --stack-name $STACK_NAME \
    --region $REGION \
    --query 'Stacks[0].Outputs[?OutputKey==`DatabaseEndpoint`].OutputValue' \
    --output text)

echo -e "${GREEN}✅ Deployment completed successfully!${NC}"
echo -e "${GREEN}🌐 Application URL: http://$LOAD_BALANCER_DNS${NC}"
echo -e "${GREEN}🗄️  Database Endpoint: $DATABASE_ENDPOINT${NC}"
echo -e "${GREEN}📦 S3 Bucket: $BUCKET_NAME${NC}"

# Create deployment summary
cat > deployment-summary.txt << EOF
EMS Tampa-FL Amptier AWS Deployment Summary
===========================================

Deployment Date: $(date)
Stack Name: $STACK_NAME
Region: $REGION
Environment: $ENVIRONMENT

Application URL: http://$LOAD_BALANCER_DNS
Database Endpoint: $DATABASE_ENDPOINT
S3 Bucket: $BUCKET_NAME

Next Steps:
1. Access the application at http://$LOAD_BALANCER_DNS
2. Configure DNS if needed
3. Set up monitoring and alerts
4. Configure SSL certificate
5. Set up backup and disaster recovery

AWS Resources Created:
- VPC with public subnets
- RDS SQL Server database
- Application Load Balancer
- Auto Scaling Group
- Security Groups
- CloudWatch Alarms
EOF

echo -e "${GREEN}📄 Deployment summary saved to deployment-summary.txt${NC}"
echo -e "${GREEN}🎉 EMS Tampa-FL Amptier is now deployed on AWS!${NC}" 