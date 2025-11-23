# Run ModCreator Tool

# Parameters:
#   -Debug   : Run in Debug mode with WPF hot reload (dotnet watch)
param(
    [switch]$Debug
)

# Set build configuration based on parameters
$Configuration = if ($Debug) { "Debug" } else { "Release" }

$ErrorActionPreference = "Stop"
# Stop script on any error

Write-Host ("=" * 60) -ForegroundColor Cyan
# Print script header
Write-Host "ModCreator - Run Script" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host ""

$scriptDir = $PSScriptRoot
# Get script directory and important paths
$buildScript = Join-Path $scriptDir "build.ps1"
$exePath = Join-Path $scriptDir "ModCreator\bin\$Configuration\net9.0-windows\ModCreator.exe"

Write-Host "Run mode: $Configuration" -ForegroundColor Magenta
# Print selected run mode
if ($Debug) {
    # Inform user about hot reload
    Write-Host "Debug mode enabled: WPF hot reload will be available if supported." -ForegroundColor Yellow
}

# Kill existing ModCreator processes
# Kill any running ModCreator.exe before starting
$existingProcesses = Get-Process -Name "ModCreator" -ErrorAction SilentlyContinue
if ($existingProcesses) {
    Write-Host "Stopping existing ModCreator processes..." -ForegroundColor Yellow
    $existingProcesses | Stop-Process -Force
    Start-Sleep -Milliseconds 500
    Write-Host "Existing processes terminated" -ForegroundColor Green
    Write-Host ""
}

# Check if build script exists
# Ensure build.ps1 exists before continuing
if (-not (Test-Path $buildScript)) {
    Write-Host "ERROR: Build script not found: $buildScript" -ForegroundColor Red
    exit 1
}

# Build using build.ps1
# Build the solution using build.ps1
Write-Host "Building solution..." -ForegroundColor Yellow
Write-Host ""

& $buildScript -Configuration $Configuration
if (-not $?) {
    Write-Host ""
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    exit 1
}

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Check if executable exists
# Ensure the built executable exists
if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: Executable not found: $exePath" -ForegroundColor Red
    Write-Host "Please build the solution first" -ForegroundColor Yellow
    exit 1
}

# Run the application
# Start the application based on selected mode
Write-Host "Starting ModCreator..." -ForegroundColor Green
Write-Host "Executable: $exePath" -ForegroundColor Gray
Write-Host ""

try {
    if ($Debug) {
        # Start dotnet watch for WPF hot reload
        Write-Host "Starting with dotnet watch run (WPF hot reload)..." -ForegroundColor Yellow
        $watchProcess = Start-Process -FilePath "dotnet" -ArgumentList "watch", "--project", "$scriptDir\ModCreator\ModCreator.csproj", "run", "--configuration", "Debug" -PassThru -WindowStyle Normal
        Start-Sleep -Seconds 2
        # Find child process ModCreator.exe
        $modProc = $null
        for ($i=0; $i -lt 30; $i++) {
            $modProc = Get-Process -Name "ModCreator" -ErrorAction SilentlyContinue
            if ($modProc) { break }
            Start-Sleep -Seconds 1
        }
        if ($modProc) {
            Write-Host "Monitoring ModCreator.exe (PID: $($modProc.Id))..." -ForegroundColor Yellow
            while ($true) {
                $modProc = Get-Process -Name "ModCreator" -ErrorAction SilentlyContinue
                if (-not $modProc) {
                    Write-Host "ModCreator.exe exited. Stopping dotnet watch..." -ForegroundColor Yellow
                    Stop-Process -Id $watchProcess.Id -Force
                    break
                }
                Start-Sleep -Seconds 1
            }
        } else {
            Write-Host "Could not find ModCreator.exe process. Press any key to stop dotnet watch manually..." -ForegroundColor Red
            $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
            Stop-Process -Id $watchProcess.Id -Force
        }
    } else {
        # Run the built executable in Release mode
        & $exePath
    }
} catch {
    # Error handling for application start
    Write-Host "ERROR: Failed to start application" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
