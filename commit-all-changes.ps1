# Comprehensive Commit Script for EMS Tampa-FL Amptier Enhanced Features
Write-Host "=== Committing Enhanced EMS Features ===" -ForegroundColor Green

# Navigate to project root
Set-Location "C:\Users\mibro\OneDrive\Desktop\EMS_Tampa-FL_Amptier"

# Check git status
Write-Host "`n1. Checking git status..." -ForegroundColor Yellow
git status

# Add all changes
Write-Host "`n2. Adding all changes..." -ForegroundColor Yellow
git add .

# Create comprehensive commit message
$commitMessage = @"
🚀 Enhanced EMS Tampa-FL Amptier with Advanced Features

✨ NEW FEATURES:
- 🔄 Incident Clustering & Management
  * Automatic grouping of similar incidents (1km radius, 30min window)
  * Multiple customer calls handling with individual access
  * Contact information and remarks tracking
  * Geographic clustering with severity assessment

- 🔗 System Integration Framework
  * SCADA, GPS, GIS, Weather service integration
  * API-first approach with multi-protocol support
  * Real-time data synchronization
  * Connection health monitoring

- 🔥 Hotspot Detection & Early Alerting
  * Automatic problem area identification
  * Configurable thresholds (3+ incidents in 2 hours)
  * Real-time alert generation
  * Severity-based classification (Low, Medium, High, Critical)

- 🏭 Combined Utility Support
  * Water and sewer operation differentiation
  * Role-based access control for separate teams
  * Domain-based incident routing
  * Utility-specific analytics and dashboards

🔧 TECHNICAL IMPROVEMENTS:
- Enhanced Incident model with clustering fields
- New SystemIntegration and IntegrationData models
- Hotspot and HotspotAlert models for problem detection
- Updated User and Equipment models for consistency
- Enhanced DbContext with all new entities
- New services: IncidentClusteringService, HotspotDetectionService, SystemIntegrationService
- Enhanced controllers with clustering and utility support
- Updated GitHub Actions CI/CD pipeline
- Enhanced AWS deployment script with monitoring

📊 API ENDPOINTS:
- GET /api/incidents/clusters - Get incident clusters
- GET /api/incidents/utility/{utilityType} - Get incidents by utility
- GET /api/hotspot - Get active hotspots
- POST /api/hotspot/detect - Detect new hotspots
- GET /api/systemintegration - Get system integrations
- POST /api/systemintegration/sync/* - Sync external data

🔒 SECURITY & COMPLIANCE:
- Enhanced audit logging
- Role-based access control
- Input validation and sanitization
- GDPR-compliant data handling

📈 PERFORMANCE:
- Optimized database queries
- Real-time SignalR updates
- Efficient clustering algorithms
- Scalable AWS infrastructure

🎯 UTILITY OPERATIONS:
- WSSC Water optimization
- Combined utility workflows
- Separate water/sewer dashboards
- Predictive alerting capabilities

📚 DOCUMENTATION:
- Comprehensive README with all features
- API endpoint documentation
- Deployment guides
- Configuration examples

🏗️ INFRASTRUCTURE:
- Updated CloudFormation template
- Enhanced Docker configuration
- Improved CI/CD pipeline
- Monitoring and alerting setup

This release addresses all requirements for combined utility operations,
multiple customer call management, system integration capabilities,
and early problem detection with real-time alerting.

Version: 2.0.0
Status: Production Ready
Environment: AWS Production
"@

# Commit with detailed message
Write-Host "`n3. Creating commit with comprehensive message..." -ForegroundColor Yellow
git commit -m $commitMessage

# Push to remote repository
Write-Host "`n4. Pushing to remote repository..." -ForegroundColor Yellow
git push origin main

# Check push status
Write-Host "`n5. Checking push status..." -ForegroundColor Yellow
git status

Write-Host "`n=== Commit and Push Completed Successfully! ===" -ForegroundColor Green
Write-Host "Enhanced EMS features have been committed and pushed to GitHub." -ForegroundColor Green
Write-Host "CI/CD pipeline will now build and test the enhanced system." -ForegroundColor Green 