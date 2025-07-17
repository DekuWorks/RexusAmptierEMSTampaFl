# PowerShell script to push CI/CD fixes
Write-Host "Pushing CI/CD fixes to GitHub..." -ForegroundColor Green

# Check current status
Write-Host "Current git status:" -ForegroundColor Yellow
git status

# Push with force if needed
Write-Host "Pushing changes..." -ForegroundColor Yellow
git push origin main --force

Write-Host "✅ Changes pushed successfully!" -ForegroundColor Green
Write-Host "Check the GitHub Actions tab to monitor the new pipeline run." -ForegroundColor Cyan 