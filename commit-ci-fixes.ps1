# PowerShell script to commit CI/CD fixes
Write-Host "Committing CI/CD pipeline fixes..." -ForegroundColor Green

# Add all changes
git add .

# Commit with descriptive message
git commit -m "Fix CI/CD pipeline issues

- Remove duplicate service registrations in Program.cs
- Add fallback connection string for CI environment
- Improve database initialization error handling
- Add SQL Server readiness check in CI pipeline
- Update appsettings.json with CI connection string
- Make database migrations non-blocking in CI

These changes should resolve the 'Initialize containers' failure
and allow the build-and-test job to complete successfully."

# Push to trigger new CI/CD run
git push origin main

Write-Host "✅ CI/CD fixes committed and pushed successfully!" -ForegroundColor Green
Write-Host "Check the GitHub Actions tab to monitor the new pipeline run." -ForegroundColor Yellow 