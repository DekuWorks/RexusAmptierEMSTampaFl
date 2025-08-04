# AWS Deployment Guide for EMS Tampa-FL Amptier

## 📋 Database Setup Status

### ✅ **Database is FULLY CONFIGURED and READY**

Your database is properly set up with:

1. **Entity Framework Core Configuration**:
   - Complete `EmsDbContext.cs` with all entities
   - Connection strings configured in `appsettings.json`
   - Migrations created and ready for deployment

2. **Database Entities**:
   - **Core EMS**: Users, Incidents, Responders, Equipment
   - **Audit & Security**: Audit logs, System integrations, Hotspots
   - **Event Management**: Events, Sessions, Registrations, Speakers
   - **Notifications**: Real-time notification system

3. **Database Options**:
   - **Development**: In-memory database (for testing)
   - **Production**: SQL Server on AWS RDS
   - **Connection Strings**: Configured for both environments

4. **Migrations Ready**:
   - Initial migration: `20250717183217_InitialCreate.cs`
   - Seed data migration: `20250717183434_StaticSeedDates.cs`
   - Model snapshot: `EmsDbContextModelSnapshot.cs`

## 🚀 AWS Deployment Process

### **Prerequisites**

Before deploying, ensure you have:

1. **AWS CLI installed and configured**
   ```bash
   aws configure
   ```

2. **Docker installed**
   ```bash
   docker --version
   ```

3. **.NET 6 SDK installed**
   ```bash
   dotnet --version
   ```

### **Step 1: Build and Test Locally**

```powershell
# Navigate to the API directory
cd RexusOps360.API

# Restore dependencies
dotnet restore

# Build the application
dotnet build -c Release

# Test the application locally
dotnet run
```

### **Step 2: Deploy to AWS**

Use the simplified deployment script:

```powershell
# From the root directory
.\deploy-aws-simple.ps1 -Environment production
```

This script will:
1. ✅ Check prerequisites (AWS CLI, Docker, .NET)
2. ✅ Build the .NET application
3. ✅ Deploy infrastructure with CloudFormation
4. ✅ Run database migrations
5. ✅ Provide deployment URL

### **Step 3: Verify Deployment**

After deployment, you'll get:
- **Application URL**: `http://[load-balancer-dns]`
- **Database Endpoint**: RDS SQL Server instance
- **Health Check**: `http://[load-balancer-dns]/health`

## 🏗️ AWS Infrastructure Components

### **What Gets Deployed**

1. **VPC & Networking**:
   - Custom VPC with public subnets
   - Internet Gateway for external access
   - Route tables for traffic routing

2. **Database (RDS)**:
   - SQL Server instance
   - Automated backups
   - Multi-AZ deployment (optional)

3. **Application Load Balancer**:
   - HTTP/HTTPS traffic distribution
   - Health checks
   - SSL termination

4. **Auto Scaling Group**:
   - EC2 instances running your application
   - Automatic scaling based on load
   - Health monitoring

5. **Security Groups**:
   - Firewall rules for application and database
   - Restricted access to database

6. **CloudWatch Monitoring**:
   - CPU utilization alarms
   - Application metrics
   - Log aggregation

## 📊 Database Configuration

### **Connection Strings**

**Development (Local)**:
```
Server=(localdb)\mssqllocaldb;Database=EmsTampaDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

**Production (AWS RDS)**:
```
Server=[rds-endpoint];Database=EmsTampaDb;User Id=admin;Password=ProductionEMS2024!;TrustServerCertificate=true;
```

### **Database Schema**

Your database includes:

1. **Users Table**: Authentication and authorization
2. **Incidents Table**: Emergency incident tracking
3. **Responders Table**: Emergency personnel management
4. **Equipment Table**: Resource inventory
5. **Audit Logs**: Security and compliance tracking
6. **Events Table**: Event management system
7. **Notifications Table**: Real-time alerts

### **Seed Data**

The database comes pre-populated with:
- Demo users (Admin: `abc` / `abc123`)
- Sample incidents and responders
- System integrations configuration
- Event management templates

## 🔧 Post-Deployment Configuration

### **1. Domain Configuration**
```bash
# Point your domain to the Load Balancer DNS
# Example: ems.tampa.gov → [load-balancer-dns]
```

### **2. SSL Certificate**
```bash
# Request SSL certificate in AWS Certificate Manager
# Configure HTTPS listener on Load Balancer
```

### **3. Environment Variables**
Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=[rds-endpoint];Database=EmsTampaDb;User Id=admin;Password=ProductionEMS2024!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "YourProductionSecretKeyHere",
    "Issuer": "RexusOps360",
    "Audience": "RexusOps360Users"
  }
}
```

### **4. Monitoring Setup**
- Configure CloudWatch alarms
- Set up log aggregation
- Enable application performance monitoring

## 🛠️ Troubleshooting

### **Common Issues**

1. **Database Connection Failed**:
   - Check security group rules
   - Verify connection string
   - Ensure RDS instance is running

2. **Application Won't Start**:
   - Check application logs
   - Verify environment variables
   - Ensure all dependencies are installed

3. **Migration Errors**:
   - Run `dotnet ef database update` manually
   - Check database permissions
   - Verify connection string format

### **Useful Commands**

```bash
# Check CloudFormation stack status
aws cloudformation describe-stacks --stack-name ems-tampa-stack

# View application logs
aws logs describe-log-groups --log-group-name-prefix /aws/ecs/ems-tampa-amptier

# Connect to RDS database
sqlcmd -S [rds-endpoint] -U admin -P ProductionEMS2024! -d EmsTampaDb
```

## 📈 Scaling and Performance

### **Auto Scaling**
- CPU utilization > 80% → Scale up
- CPU utilization < 30% → Scale down
- Minimum: 1 instance
- Maximum: 3 instances

### **Database Scaling**
- Start with `db.t3.micro` (free tier)
- Scale to `db.t3.small` or `db.t3.medium` for production
- Enable Multi-AZ for high availability

### **Performance Optimization**
- Enable connection pooling
- Use read replicas for heavy read workloads
- Implement caching (Redis) for frequently accessed data

## 🔒 Security Considerations

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

## 💰 Cost Estimation

### **Monthly Costs (US East 1)**
- **RDS SQL Server**: $15-30/month
- **EC2 Instances**: $15-45/month
- **Load Balancer**: $20/month
- **Data Transfer**: $5-15/month
- **CloudWatch**: $5-10/month

**Total Estimated Cost**: $60-120/month

## 📞 Support and Maintenance

### **Regular Maintenance**
- Database backups (automated)
- Security patches (automated)
- Performance monitoring
- Log rotation

### **Emergency Procedures**
- Database restore procedures
- Application rollback process
- Incident response plan

---

## 🎯 Quick Start Commands

```powershell
# 1. Test locally
cd RexusOps360.API
dotnet run

# 2. Deploy to AWS
cd ..
.\deploy-aws-simple.ps1 -Environment production

# 3. Check deployment status
aws cloudformation describe-stacks --stack-name ems-tampa-stack

# 4. Access application
# Use the Load Balancer DNS provided after deployment
```

Your EMS Tampa-FL Amptier system is now ready for production deployment on AWS! 🚀 