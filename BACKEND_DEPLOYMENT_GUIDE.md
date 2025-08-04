# 🚀 EMS Backend Deployment Guide

## 📋 Pre-Deployment Checklist

### ✅ **Prerequisites Verified**
- [x] .NET SDK 9.0.203 installed
- [x] AWS CLI 2.27.53 configured
- [x] Application builds successfully
- [x] Database migrations ready
- [x] Production configuration created

### 🔧 **Backend Status**
- [x] Clean build completed
- [x] Release publish successful
- [x] Deployment package created
- [x] Production config ready

## 🎯 **Deployment Options**

### **Option 1: CloudFormation Deployment (Recommended)**

This is the most reliable method for Windows environments:

```powershell
# Navigate to the API directory
cd RexusOps360.API

# Run the deployment script
.\deploy-backend-aws.ps1 -Environment production -Region us-east-1
```

**What this does:**
1. ✅ Builds the application in Release mode
2. ✅ Creates deployment package
3. ✅ Uploads to S3
4. ✅ Deploys infrastructure with CloudFormation
5. ✅ Runs database migrations
6. ✅ Provides deployment URLs

### **Option 2: Manual AWS CLI Deployment**

If you prefer manual control:

```powershell
# 1. Build and package
dotnet clean
dotnet publish -c Release -o ./publish

# 2. Create deployment zip
Compress-Archive -Path "./publish/*" -DestinationPath "./ems-backend-deploy.zip"

# 3. Upload to S3
aws s3 mb s3://ems-tampa-deployments-us-east-1
aws s3 cp "./ems-backend-deploy.zip" "s3://ems-tampa-deployments-us-east-1/"

# 4. Deploy infrastructure
aws cloudformation deploy --template-file "../aws-deploy.yml" --stack-name ems-backend-stack --capabilities CAPABILITY_NAMED_IAM --region us-east-1
```

### **Option 3: Elastic Beanstalk (Alternative)**

If EB CLI works on your system:

```bash
# Initialize EB CLI
eb init

# Create environment
eb create ems-api-env

# Deploy
eb deploy

# Open URL
eb open
```

## 🏗️ **Infrastructure Components**

### **What Gets Deployed**

1. **VPC & Networking**:
   - Custom VPC with public subnets
   - Internet Gateway
   - Route tables

2. **Database (RDS)**:
   - SQL Server instance
   - Automated backups
   - Security groups

3. **Application Load Balancer**:
   - HTTP/HTTPS traffic distribution
   - Health checks
   - SSL termination (when configured)

4. **Auto Scaling Group**:
   - EC2 instances running your .NET app
   - Automatic scaling
   - Health monitoring

5. **Security Groups**:
   - Firewall rules
   - Database access restrictions

## 📊 **Database Configuration**

### **Connection Strings**

**Production (AWS RDS)**:
```
Server=[rds-endpoint];Database=EmsTampaDb;User Id=admin;Password=ProductionEMS2024!;TrustServerCertificate=true;
```

### **Database Schema**

Your database includes:
- ✅ Users (Authentication)
- ✅ Incidents (Emergency tracking)
- ✅ Responders (Personnel management)
- ✅ Equipment (Resource inventory)
- ✅ Audit Logs (Security tracking)
- ✅ Events (Event management)
- ✅ Notifications (Real-time alerts)

### **Seed Data**

Pre-populated with:
- Demo users (Admin: `abc` / `abc123`)
- Sample incidents and responders
- System integrations
- Event templates

## 🔧 **Environment Variables**

### **Production Configuration**

The deployment automatically configures:

```json
{
  "Environment": "Production",
  "ConnectionStrings": {
    "DefaultConnection": "Server=[RDS_ENDPOINT];Database=EmsTampaDb;User Id=admin;Password=ProductionEMS2024!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "YourProductionSecretKeyHere12345678901234567890",
    "Issuer": "RexusOps360",
    "Audience": "RexusOps360Users"
  }
}
```

### **Security Considerations**

- 🔒 JWT keys should be changed for production
- 🔒 Database passwords should be strong
- 🔒 SSL certificates should be configured
- 🔒 CORS settings should be restricted

## 🚀 **Deployment Process**

### **Step 1: Prepare Backend**

```powershell
cd RexusOps360.API
dotnet clean
dotnet publish -c Release -o ./publish
```

### **Step 2: Deploy to AWS**

```powershell
# Option A: Use deployment script (Recommended)
.\deploy-backend-aws.ps1 -Environment production

# Option B: Manual deployment
aws cloudformation deploy --template-file "../aws-deploy.yml" --stack-name ems-backend-stack --capabilities CAPABILITY_NAMED_IAM
```

### **Step 3: Verify Deployment**

After deployment, you'll get:
- **Backend URL**: `http://[load-balancer-dns]`
- **Health Check**: `http://[load-balancer-dns]/health`
- **API Docs**: `http://[load-balancer-dns]/swagger`
- **Database**: RDS SQL Server instance

### **Step 4: Test Endpoints**

```bash
# Health check
curl http://[load-balancer-dns]/health

# API documentation
curl http://[load-balancer-dns]/swagger

# Test authentication
curl -X POST http://[load-balancer-dns]/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"abc","password":"abc123"}'
```

## 📈 **Monitoring & Scaling**

### **Auto Scaling**
- CPU utilization > 80% → Scale up
- CPU utilization < 30% → Scale down
- Minimum: 1 instance
- Maximum: 3 instances

### **Database Scaling**
- Start with `db.t3.micro` (free tier)
- Scale to `db.t3.small` or `db.t3.medium` for production
- Enable Multi-AZ for high availability

### **Performance Monitoring**
- CloudWatch metrics
- Application logs
- Database performance
- Response times

## 🔒 **Security Best Practices**

### **Network Security**
- Database in private subnet
- Application in public subnet
- Security groups restrict access

### **Data Protection**
- Encrypted connections (TLS)
- Encrypted storage (RDS encryption)
- Regular security updates

### **Access Control**
- IAM roles for EC2 instances
- Database user with minimal privileges
- JWT token authentication

## 🛠️ **Troubleshooting**

### **Common Issues**

1. **Build Errors**:
   ```powershell
   dotnet restore
   dotnet build -c Release
   ```

2. **Deployment Failures**:
   ```powershell
   aws cloudformation describe-stack-events --stack-name ems-backend-stack
   ```

3. **Database Connection**:
   ```powershell
   # Check RDS endpoint
   aws cloudformation describe-stacks --stack-name ems-backend-stack --query 'Stacks[0].Outputs'
   ```

4. **Application Logs**:
   ```powershell
   # View CloudWatch logs
   aws logs describe-log-groups --log-group-name-prefix /aws/ecs/ems-tampa-amptier
   ```

### **Useful Commands**

```powershell
# Check stack status
aws cloudformation describe-stacks --stack-name ems-backend-stack

# View application logs
aws logs describe-log-groups --log-group-name-prefix /aws/ecs/ems-tampa-amptier

# Connect to RDS database
sqlcmd -S [rds-endpoint] -U admin -P ProductionEMS2024! -d EmsTampaDb

# Test API endpoints
curl http://[load-balancer-dns]/health
```

## 💰 **Cost Estimation**

### **Monthly Costs (US East 1)**
- **RDS SQL Server**: $15-30/month
- **EC2 Instances**: $15-45/month
- **Load Balancer**: $20/month
- **Data Transfer**: $5-15/month
- **CloudWatch**: $5-10/month

**Total Estimated Cost**: $60-120/month

## 📞 **Post-Deployment**

### **Next Steps**

1. **Test the API**:
   - Health check endpoint
   - Authentication endpoints
   - Database operations

2. **Update Frontend**:
   - Point to new backend URL
   - Test all functionality
   - Update CORS settings

3. **Configure SSL**:
   - Request SSL certificate
   - Configure HTTPS listener
   - Update DNS settings

4. **Set up Monitoring**:
   - CloudWatch alarms
   - Application performance monitoring
   - Log aggregation

---

## 🎉 **Ready to Deploy!**

Your EMS backend is ready for production deployment on AWS! 🚀

**Demo Credentials**: `abc` / `abc123` (Admin)
**Deployment Script**: `deploy-backend-aws.ps1`
**Infrastructure**: CloudFormation template ready
**Database**: Migrations and seed data prepared 