# 🚀 AWS Deployment Steps for EMS Backend

## 📋 **Prerequisites Checklist**
- [x] .NET SDK 9.0.203 installed
- [x] AWS CLI 2.27.53 installed
- [x] Application builds successfully
- [x] Database migrations ready
- [x] Production configuration created

## 🔐 **Step 1: Configure AWS Credentials**

### Option A: Interactive Configuration
```powershell
aws configure
```
**Enter when prompted:**
- AWS Access Key ID: `your-access-key`
- AWS Secret Access Key: `your-secret-key`
- Default region: `us-east-1` (or your preferred region)
- Default output format: `json`

### Option B: Environment Variables
```powershell
$env:AWS_ACCESS_KEY_ID="your-access-key"
$env:AWS_SECRET_ACCESS_KEY="your-secret-key"
$env:AWS_DEFAULT_REGION="us-east-1"
```

### Option C: AWS Profile
```powershell
aws configure --profile ems-deployment
```

## 🏗️ **Step 2: Prepare Backend for Deployment**

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

## 📦 **Step 3: Deploy Using CloudFormation**

### Create S3 Bucket for Deployment
```powershell
aws s3 mb s3://ems-tampa-deployment-bucket
```

### Deploy Infrastructure
```powershell
# Deploy the CloudFormation stack
aws cloudformation create-stack \
  --stack-name ems-backend-stack \
  --template-body file://../aws-deploy.yml \
  --capabilities CAPABILITY_IAM \
  --parameters ParameterKey=Environment,ParameterValue=production
```

### Monitor Deployment
```powershell
aws cloudformation describe-stacks --stack-name ems-backend-stack
```

## 🐳 **Step 4: Deploy Using Docker**

### Build Docker Image
```powershell
docker build -t ems-backend:latest .
```

### Tag for ECR
```powershell
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin your-account-id.dkr.ecr.us-east-1.amazonaws.com
docker tag ems-backend:latest your-account-id.dkr.ecr.us-east-1.amazonaws.com/ems-backend:latest
```

### Push to ECR
```powershell
docker push your-account-id.dkr.ecr.us-east-1.amazonaws.com/ems-backend:latest
```

## 🌐 **Step 5: Deploy to Elastic Beanstalk**

### Create EB Application
```powershell
eb init ems-backend --platform dotnet --region us-east-1
```

### Create Environment
```powershell
eb create ems-api-env --instance-type t3.small --single-instance
```

### Deploy Application
```powershell
eb deploy
```

## 🔧 **Step 6: Configure Environment Variables**

```powershell
eb setenv ASPNETCORE_ENVIRONMENT=Production
eb setenv DB_CONNECTION="your-rds-connection-string"
eb setenv JWT_SECRET="your-production-jwt-secret"
```

## 🗄️ **Step 7: Database Setup**

### Option A: RDS via CloudFormation
The `aws-deploy.yml` template includes RDS setup.

### Option B: Manual RDS Setup
```powershell
# Create RDS instance
aws rds create-db-instance \
  --db-instance-identifier ems-database \
  --db-instance-class db.t3.micro \
  --engine sqlserver-ex \
  --master-username admin \
  --master-user-password your-password \
  --allocated-storage 20
```

### Run Migrations
```powershell
# Connect to your deployed backend and run:
dotnet ef database update --environment Production
```

## ✅ **Step 8: Test Deployment**

### Health Check
```powershell
curl https://your-eb-url.elasticbeanstalk.com/api/health
```

### Test Login
```powershell
curl -X POST https://your-eb-url.elasticbeanstalk.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"abc","password":"abc123"}'
```

### Test Incidents API
```powershell
curl https://your-eb-url.elasticbeanstalk.com/api/incidents
```

## 🌍 **Step 9: Update Frontend Configuration**

### Update API URLs
Replace all instances of `http://localhost:5000` with your deployed URL:

```javascript
// In all frontend files
const API_BASE_URL = "https://your-eb-url.elasticbeanstalk.com";
```

### Files to Update:
- `frontend/dashboard.html`
- `frontend/login.html`
- `frontend/incident-management.html`
- All other frontend files with API calls

## 📊 **Step 10: Monitoring & Logs**

### Enable CloudWatch
```powershell
eb config
# Enable enhanced health reporting
```

### View Logs
```powershell
eb logs
```

### Monitor Metrics
```powershell
aws cloudwatch get-metric-statistics \
  --namespace AWS/ElasticBeanstalk \
  --metric-name CPUUtilization \
  --dimensions Name=EnvironmentName,Value=ems-api-env \
  --start-time 2024-01-15T00:00:00Z \
  --end-time 2024-01-15T23:59:59Z \
  --period 300 \
  --statistics Average
```

## 🔒 **Step 11: Security Configuration**

### Update Security Groups
```powershell
aws ec2 describe-security-groups --group-names ems-backend-sg
```

### Configure SSL Certificate
```powershell
# If using custom domain
aws acm import-certificate \
  --certificate file://certificate.pem \
  --private-key file://private-key.pem \
  --certificate-chain file://chain.pem
```

## 📝 **Step 12: Documentation**

### Update README
```markdown
## Live Demo
- **Backend URL**: https://your-eb-url.elasticbeanstalk.com
- **Admin Login**: abc / abc123
- **API Documentation**: https://your-eb-url.elasticbeanstalk.com/swagger
```

### Create Deployment Summary
```markdown
# Deployment Summary
- **Environment**: Production
- **Region**: us-east-1
- **Backend URL**: https://your-eb-url.elasticbeanstalk.com
- **Database**: RDS SQL Server
- **Monitoring**: CloudWatch enabled
- **SSL**: Configured
```

## 🚨 **Troubleshooting**

### Common Issues:

1. **AWS Credentials Not Found**
   ```powershell
   aws sts get-caller-identity
   ```

2. **Build Failures**
   ```powershell
   dotnet clean
   dotnet restore
   dotnet build --verbosity detailed
   ```

3. **Database Connection Issues**
   ```powershell
   # Check RDS status
   aws rds describe-db-instances --db-instance-identifier ems-database
   ```

4. **EB CLI Issues**
   ```powershell
   # Reinstall EB CLI
   pip uninstall awsebcli
   pip install awsebcli --upgrade --user
   ```

## 🎯 **Quick Deployment Commands**

```powershell
# Complete deployment in one go
aws configure
cd RexusOps360.API
dotnet publish -c Release -o ./publish
eb init ems-backend --platform dotnet --region us-east-1
eb create ems-api-env --instance-type t3.small --single-instance
eb deploy
eb setenv ASPNETCORE_ENVIRONMENT=Production
```

## 📞 **Support**

If you encounter issues:
1. Check CloudWatch logs: `eb logs`
2. Verify AWS credentials: `aws sts get-caller-identity`
3. Test local build: `dotnet run`
4. Check EB status: `eb status` 