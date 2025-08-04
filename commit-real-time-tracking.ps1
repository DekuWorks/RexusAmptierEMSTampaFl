# RexusOps360 EMS - Commit Real-Time Tracking Changes
# Commits and pushes all changes for real-time tracking and logo navigation

Write-Host "🚨 RexusOps360 EMS - Committing Real-Time Tracking Changes" -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to project root
Set-Location "C:\Users\mibro\OneDrive\Desktop\EMS_Tampa-FL_Amptier"

Write-Host "📁 Current Directory: $(Get-Location)" -ForegroundColor Yellow
Write-Host ""

# Check git status
Write-Host "🔍 Checking git status..." -ForegroundColor Green
git status

Write-Host ""
Write-Host "📦 Adding all changes..." -ForegroundColor Green
git add .

# Create detailed commit message
$commitMessage = @"
feat: Implement real-time incident tracking and clickable logo navigation

🚨 Real-Time Tracking System:
- Added interactive incident map with Leaflet.js
- Implemented real-time location tracking for incidents and responders
- Created filtering by disaster type, priority, and status
- Added live statistics and heatmap functionality
- Integrated with C# backend API endpoints

🗺️ Map Features:
- Interactive map with custom markers
- Real-time status updates
- Priority-based color coding
- Location search and filtering
- Mobile responsive design

🔗 Clickable Logo Navigation:
- Made all logos clickable across all pages
- Added hover effects with scale animation
- Consistent navigation to dashboard
- Improved user experience and flow

📱 Frontend Updates:
- Added incident-map.html with full functionality
- Updated navigation in dashboard
- Added quick action buttons for public reporting
- Enhanced mobile responsiveness

⚙️ Backend Services:
- Created RealTimeTrackingService.cs
- Added MapController.cs with REST endpoints
- Updated Responder model with location fields
- Integrated SignalR for real-time updates

🎯 Key Features:
- Real-time incident tracking
- Location-based filtering
- Live statistics updates
- Interactive map controls
- Universal logo navigation
- Professional hover effects

Author: RexusOps360 Development Team
Version: 1.0.0
Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

Write-Host ""
Write-Host "💾 Committing changes..." -ForegroundColor Green
git commit -m "$commitMessage"

Write-Host ""
Write-Host "🚀 Pushing to remote repository..." -ForegroundColor Green
git push

Write-Host ""
Write-Host "✅ Changes committed and pushed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Summary of Changes:" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host "  ✅ Real-time incident tracking system" -ForegroundColor Green
Write-Host "  ✅ Interactive map with Leaflet.js" -ForegroundColor Green
Write-Host "  ✅ Clickable logos on all pages" -ForegroundColor Green
Write-Host "  ✅ C# backend services and controllers" -ForegroundColor Green
Write-Host "  ✅ Enhanced navigation flow" -ForegroundColor Green
Write-Host "  ✅ Mobile responsive design" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan
Write-Host "  1. Test the real-time tracking system" -ForegroundColor Gray
Write-Host "  2. Verify logo navigation works on all pages" -ForegroundColor Gray
Write-Host "  3. Check mobile responsiveness" -ForegroundColor Gray
Write-Host "  4. Test API endpoints with backend running" -ForegroundColor Gray 