# 🚀 Quick EMS Backend Deployment Guide

## 📋 **Prerequisites**
- ✅ .NET SDK 9.0.203 installed
- ✅ AWS CLI 2.27.53 installed
- ✅ Application builds successfully
- ⚠️ **AWS Credentials needed** (see Step 1)

## 🔐 **Step 1: Configure AWS Credentials**

### Option A: Interactive Configuration (Recommended)
```powershell
aws configure
```
**Enter when prompted:**
- AWS Access Key ID: `your-access-key`
- AWS Secret Access Key: `your-secret-key`
- Default region: `us-east-1`
- Default output format: `json`

### Option B: Environment Variables
```powershell
$env:AWS_ACCESS_KEY_ID="your-access-key"
$env:AWS_SECRET_ACCESS_KEY="your-secret-key"
$env:AWS_DEFAULT_REGION="us-east-1"
```

### Option C: Use Helper Script
```powershell
.\configure-aws.ps1
```

### Verify Configuration
```powershell
aws sts get-caller-identity
```
**Expected output:**
```json
{
    "UserId": "AIDACKCEVSQ6C2EXAMPLE",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/YourUsername"
}
```

## 🏗️ **Step 2: Prepare Backend**

```powershell
# Navigate to API directory
cd RexusOps360.API

# Clean and build
dotnet clean
dotnet restore
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish
```

## 🚀 **Step 3: Deploy Backend**

### Option A: CloudFormation Deployment (Recommended)
```powershell
# Run the deployment script
.\deploy-simple.ps1
```

### Option B: Elastic Beanstalk Deployment
```powershell
# Initialize EB application
eb init ems-backend --platform dotnet --region us-east-1

# Create environment
eb create ems-api-env --instance-type t3.small --single-instance

# Deploy application
eb deploy

# Set environment variables
eb setenv ASPNETCORE_ENVIRONMENT=Production
```

## ✅ **Step 4: Test Deployment**

### Get Your Deployment URL
- **CloudFormation**: Check AWS Console → CloudFormation → Stacks → ems-backend-stack → Outputs
- **Elastic Beanstalk**: Check AWS Console → Elastic Beanstalk → Environments → ems-api-env

### Test Health Endpoint
```powershell
curl https://your-deployment-url/api/health
```

### Test Login
```powershell
curl -X POST https://your-deployment-url/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"abc","password":"abc123"}'
```

### Test Incidents API
```powershell
curl https://your-deployment-url/api/incidents
```

## 🌍 **Step 5: Update Frontend**

### Update API URLs in Frontend Files
Replace all instances of `http://localhost:5000` with your deployed URL:

**Files to update:**
- `frontend/dashboard.html`
- `frontend/login.html`
- `frontend/incident-management.html`
- `frontend/create-incident.html`
- `frontend/admin.html`

**Example:**
```javascript
// Old
const API_BASE_URL = "http://localhost:5000";

// New
const API_BASE_URL = "https://your-deployment-url.com";
```

## 📊 **Step 6: Monitor & Verify**

### Check Application Logs
```powershell
# For Elastic Beanstalk
eb logs

# For CloudFormation (check CloudWatch)
aws logs describe-log-groups --log-group-name-prefix /aws/elasticbeanstalk
```

### Monitor Health
```powershell
# Check application health
curl https://your-deployment-url/api/health

# Check database connection
curl https://your-deployment-url/api/incidents
```

## 🔒 **Step 7: Security & SSL**

### Configure SSL Certificate (Optional)
```powershell
# If using custom domain
aws acm import-certificate `
  --certificate file://certificate.pem `
  --private-key file://private-key.pem `
  --certificate-chain file://chain.pem
```

### Update Security Groups
```powershell
# Check current security groups
aws ec2 describe-security-groups --group-names ems-backend-sg
```

## 📝 **Step 8: Documentation**

### Update README.md
```markdown
## Live Demo
- **Backend URL**: https://your-deployment-url.com
- **Admin Login**: abc / abc123
- **API Documentation**: https://your-deployment-url.com/swagger
```

### Create Deployment Summary
```markdown
# Deployment Summary
- **Environment**: Production
- **Region**: us-east-1
- **Backend URL**: https://your-deployment-url.com
- **Database**: RDS SQL Server
- **Monitoring**: CloudWatch enabled
- **SSL**: Configured
```

## 🚨 **Troubleshooting**

### Common Issues:

1. **AWS Credentials Not Found**
   ```powershell
   aws sts get-caller-identity
   # If fails, run: aws configure
   ```

2. **Build Failures**
   ```powershell
   dotnet clean
   dotnet restore
   dotnet build --verbosity detailed
   ```

3. **Deployment Timeout**
   ```powershell
   # Check CloudFormation status
   aws cloudformation describe-stacks --stack-name ems-backend-stack
   
   # Check EB status
   eb status
   ```

4. **Database Connection Issues**
   ```powershell
   # Check RDS status
   aws rds describe-db-instances --db-instance-identifier ems-database
   ```

5. **API Not Responding**
   ```powershell
   # Check security groups
   aws ec2 describe-security-groups --group-names ems-backend-sg
   
   # Check load balancer
   aws elbv2 describe-load-balancers
   ```

## 🎯 **Quick Commands Summary**

```powershell
# 1. Configure AWS
aws configure

# 2. Verify credentials
aws sts get-caller-identity

# 3. Build application
cd RexusOps360.API
dotnet build -c Release

# 4. Deploy
.\deploy-simple.ps1

# 5. Test deployment
curl https://your-deployment-url/api/health

# 6. Update frontend URLs
# (Manual: Replace localhost:5000 with your deployment URL)
```

## 📞 **Support**

If you encounter issues:
1. Check CloudWatch logs: `eb logs` or AWS Console
2. Verify AWS credentials: `aws sts get-caller-identity`
3. Test local build: `dotnet run`
4. Check deployment status: `eb status` or CloudFormation console

## 🎉 **Success Indicators**

✅ **Deployment Successful When:**
- Health endpoint returns 200 OK
- Login with abc/abc123 works
- Incidents API returns data
- Frontend connects to new backend URL
- SSL certificate configured (if using custom domain)
- Monitoring and logs working

**Your EMS backend is now live on AWS! 🚀** 