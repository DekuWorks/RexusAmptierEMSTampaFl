# EMS Tampa-FL Amptier AWS Deployment Guide

## Overview

This guide provides step-by-step instructions for deploying the EMS Tampa-FL Amptier Emergency Management System on AWS using CloudFormation, Docker, and modern DevOps practices.

## 🏗️ Architecture

The deployment creates a production-ready infrastructure with:

- **VPC** with public subnets across multiple availability zones
- **RDS SQL Server** database for data persistence
- **Application Load Balancer** for high availability
- **Auto Scaling Group** for scalability
- **EC2 instances** running Docker containers
- **CloudWatch** monitoring and alarms
- **Security Groups** for network security

## 📋 Prerequisites

### 1. AWS Account Setup
- AWS account with appropriate permissions
- AWS CLI installed and configured
- IAM user with CloudFormation, EC2, RDS, and S3 permissions

### 2. Local Development Environment
- Docker and Docker Compose installed
- .NET 9.0 SDK
- Git for version control

### 3. AWS CLI Configuration
```bash
aws configure
# Enter your AWS Access Key ID
# Enter your AWS Secret Access Key
# Enter your default region (e.g., us-east-1)
# Enter your output format (json)
```

## 🚀 Quick Deployment

### Option 1: Automated Deployment (Recommended)

1. **Clone and prepare the repository:**
```bash
git clone <your-repo-url>
cd EMS_Tampa-FL_Amptier
```

2. **Make the deployment script executable:**
```bash
chmod +x deploy-aws.sh
```

3. **Run the deployment:**
```bash
./deploy-aws.sh
```

### Option 2: Manual Deployment

1. **Create S3 bucket for artifacts:**
```bash
BUCKET_NAME="ems-tampa-deployment-$(date +%s)"
aws s3 mb s3://$BUCKET_NAME --region us-east-1
```

2. **Upload application files:**
```bash
tar -czf ems-app.tar.gz -C . .
aws s3 cp ems-app.tar.gz s3://$BUCKET_NAME/
aws s3 cp docker-compose.yml s3://$BUCKET_NAME/
aws s3 cp Dockerfile s3://$BUCKET_NAME/
```

3. **Deploy CloudFormation stack:**
```bash
aws cloudformation deploy \
    --template-file aws-deploy.yml \
    --stack-name ems-tampa-production \
    --parameter-overrides Environment=Production InstanceType=t3.medium \
    --capabilities CAPABILITY_IAM \
    --region us-east-1
```

## 🔧 Configuration

### Environment Variables

The application uses the following environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=<db-endpoint>;Database=EmsTampaDb;User Id=admin;Password=<password>;TrustServerCertificate=true
```

### Database Configuration

- **Engine:** SQL Server Express
- **Instance Class:** db.t3.micro (free tier eligible)
- **Storage:** 20GB GP2
- **Backup Retention:** 7 days
- **Multi-AZ:** Disabled (for cost optimization)

### Application Configuration

- **Instance Type:** t3.medium (2 vCPU, 4GB RAM)
- **Auto Scaling:** 1-3 instances
- **Health Check:** /health endpoint
- **Load Balancer:** Application Load Balancer

## 📊 Monitoring and Logging

### CloudWatch Alarms

The deployment creates the following alarms:
- **CPU Utilization:** Triggers when CPU > 80% for 2 periods
- **Database Connections:** Monitors RDS connection count
- **Application Health:** Load balancer health checks

### Logging

- **Application Logs:** Available in CloudWatch Logs
- **Database Logs:** RDS logs in CloudWatch
- **Access Logs:** Load balancer access logs

## 🔒 Security

### Network Security
- **VPC:** Isolated network environment
- **Security Groups:** Restrictive firewall rules
- **Subnets:** Public subnets for load balancer, private for database

### Data Security
- **Encryption:** RDS encryption at rest
- **SSL/TLS:** Application load balancer SSL termination
- **IAM:** Least privilege access

### Application Security
- **JWT Authentication:** Secure API endpoints
- **Audit Logging:** Comprehensive security event logging
- **Input Validation:** Server-side validation

## 🧪 Testing

### Local Testing
```bash
# Build and run locally
docker-compose up -d

# Test endpoints
curl http://localhost:5169/health
curl http://localhost:5169/api/incidents
```

### Production Testing
```bash
# Get the load balancer DNS
LOAD_BALANCER_DNS=$(aws cloudformation describe-stacks \
    --stack-name ems-tampa-production \
    --query 'Stacks[0].Outputs[?OutputKey==`LoadBalancerDNS`].OutputValue' \
    --output text)

# Test the application
curl http://$LOAD_BALANCER_DNS/health
curl http://$LOAD_BALANCER_DNS/api/incidents
```

## 🔄 CI/CD Integration

### GitHub Actions

The repository includes a CI/CD pipeline that:
1. Builds the application
2. Runs tests
3. Creates Docker image
4. Deploys to AWS

### Manual Deployment Updates

To update the application:

1. **Build new Docker image:**
```bash
docker build -t ems-tampa:latest .
```

2. **Update the deployment:**
```bash
aws cloudformation deploy \
    --template-file aws-deploy.yml \
    --stack-name ems-tampa-production \
    --capabilities CAPABILITY_IAM
```

## 🗄️ Database Management

### Backup and Recovery
- **Automated Backups:** Daily backups with 7-day retention
- **Manual Snapshots:** Create point-in-time recovery
- **Cross-Region Replication:** For disaster recovery

### Migration
```bash
# Apply database migrations
dotnet ef database update --connection "Server=<db-endpoint>;Database=EmsTampaDb;User Id=admin;Password=<password>"
```

## 💰 Cost Optimization

### Estimated Monthly Costs (us-east-1)
- **EC2 t3.medium:** ~$30/month
- **RDS db.t3.micro:** ~$15/month
- **Application Load Balancer:** ~$20/month
- **Data Transfer:** ~$5/month
- **CloudWatch:** ~$5/month
- **Total:** ~$75/month

### Cost Reduction Strategies
1. **Use Spot Instances** for non-critical workloads
2. **Reserved Instances** for predictable usage
3. **S3 Lifecycle Policies** for artifact cleanup
4. **CloudWatch Logs retention** policies

## 🚨 Troubleshooting

### Common Issues

1. **Stack Creation Fails**
   - Check IAM permissions
   - Verify VPC limits
   - Review CloudFormation events

2. **Application Not Accessible**
   - Check security group rules
   - Verify load balancer health checks
   - Review application logs

3. **Database Connection Issues**
   - Verify security group allows 1433
   - Check RDS endpoint
   - Validate connection string

### Useful Commands

```bash
# Check stack status
aws cloudformation describe-stacks --stack-name ems-tampa-production

# View stack events
aws cloudformation describe-stack-events --stack-name ems-tampa-production

# Get load balancer DNS
aws cloudformation describe-stacks \
    --stack-name ems-tampa-production \
    --query 'Stacks[0].Outputs[?OutputKey==`LoadBalancerDNS`].OutputValue' \
    --output text

# SSH to EC2 instance (if needed)
aws ec2 describe-instances --filters "Name=tag:Name,Values=Production-EMS-Instance"
```

## 📞 Support

For deployment issues:
1. Check CloudFormation events
2. Review CloudWatch logs
3. Verify AWS service limits
4. Contact AWS support if needed

## 🔄 Updates and Maintenance

### Regular Maintenance Tasks
1. **Security Updates:** Monthly OS and application updates
2. **Database Maintenance:** Weekly backup verification
3. **Performance Monitoring:** Daily CloudWatch metrics review
4. **Cost Review:** Monthly cost analysis

### Scaling Considerations
- **Horizontal Scaling:** Auto Scaling Group handles traffic spikes
- **Vertical Scaling:** Upgrade instance types as needed
- **Database Scaling:** RDS read replicas for read-heavy workloads

---

**Last Updated:** July 2024  
**Version:** 1.0  
**AWS Region:** us-east-1 