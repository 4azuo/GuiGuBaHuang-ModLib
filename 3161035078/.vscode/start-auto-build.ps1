# Start Auto Build and Deploy
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting Auto Build and Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check if Python is installed
$pythonCmd = Get-Command python -ErrorAction SilentlyContinue
if (-not $pythonCmd) {
    Write-Host "❌ Python not found! Please install Python first." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Python found: $($pythonCmd.Version)" -ForegroundColor Green

# Install requirements if watchdog is not installed
Write-Host "`nChecking dependencies..." -ForegroundColor Yellow
$watchdogCheck = python -c "import watchdog" 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Installing watchdog module..." -ForegroundColor Yellow
    pip install watchdog
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install watchdog!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ watchdog installed successfully" -ForegroundColor Green
} else {
    Write-Host "✓ watchdog already installed" -ForegroundColor Green
}

# Run the Python script
Write-Host "`nStarting auto-build monitor..." -ForegroundColor Cyan
$scriptPath = Join-Path $PSScriptRoot "auto-build-and-deploy.py"
python $scriptPath
