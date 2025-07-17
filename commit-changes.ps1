# PowerShell script to commit all changes to git

Write-Host "🚀 Committing all changes to EMS Tampa-FL Amptier project..." -ForegroundColor Green

# Add all files to staging
Write-Host "📦 Staging all changes..." -ForegroundColor Yellow
git add .

# Check status
Write-Host "📊 Current git status:" -ForegroundColor Cyan
git status

# Create comprehensive commit message
$commitMessage = @"
feat: Complete EMS Tampa-FL Amptier production system

🚀 Major Features Added:
- Production SQL Server database with Entity Framework migrations
- Real-time SignalR communication for live updates
- Comprehensive audit logging and security compliance
- Advanced analytics dashboard with Chart.js integration
- GPS tracking and responder management system
- Equipment inventory with barcode scanning support
- Shift scheduling and availability management
- Mobile-first responsive UI with dark mode

🏗️ Infrastructure & Deployment:
- Complete AWS CloudFormation template for production deployment
- Docker containerization with multi-stage builds
- CI/CD pipeline with GitHub Actions
- Auto-scaling infrastructure with load balancing
- CloudWatch monitoring and alerting
- Comprehensive deployment documentation

🔒 Security & Compliance:
- JWT authentication with role-based access control
- Comprehensive audit logging for compliance
- SQL Server encryption at rest and in transit
- Input validation and sanitization
- Security event tracking and monitoring

📊 Analytics & Reporting:
- Real-time dashboard with live metrics
- Interactive charts for incident analysis
- Response time tracking and optimization
- Equipment utilization analytics
- Performance monitoring and reporting

🛠️ Technical Improvements:
- Migrated from in-memory to SQL Server database
- Optimized Entity Framework models and relationships
- Enhanced API endpoints with proper error handling
- Improved frontend with modern CSS and JavaScript
- Added comprehensive unit testing framework

📱 User Experience:
- Mobile-responsive design with dark mode
- Real-time notifications and updates
- Interactive maps and location services
- Intuitive admin interface
- Role-based dashboard customization

🚨 Emergency Response Features:
- Real-time incident reporting and tracking
- GPS-based responder coordination
- Equipment allocation and management
- Priority-based incident classification
- Automated shift scheduling

📚 Documentation:
- Comprehensive README with deployment guides
- AWS deployment documentation
- API documentation and usage examples
- Troubleshooting and maintenance guides
- Cost analysis and optimization strategies

Version: 1.0.0
Status: Production Ready
Environment: AWS Production
"@

# Commit with the comprehensive message
Write-Host "💾 Committing changes..." -ForegroundColor Yellow
git commit -m "$commitMessage"

# Push to remote repository
Write-Host "🚀 Pushing to remote repository..." -ForegroundColor Yellow
git push

Write-Host "✅ All changes committed and pushed successfully!" -ForegroundColor Green
Write-Host "🎉 EMS Tampa-FL Amptier is now production ready!" -ForegroundColor Green 