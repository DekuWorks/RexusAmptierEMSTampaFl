# Test password hashing to verify the correct hash for "pass123"
Write-Host "Testing password hashing..." -ForegroundColor Green

# The hash in the C# code is: "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg="
# Let's verify this is correct for "pass123"

$password = "pass123"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash($bytes)
$hash = [System.Convert]::ToBase64String($hashBytes)

Write-Host "Password: $password" -ForegroundColor Yellow
Write-Host "Generated Hash: $hash" -ForegroundColor Yellow
Write-Host "Expected Hash: jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=" -ForegroundColor Yellow
Write-Host "Match: $($hash -eq 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=')" -ForegroundColor Green 