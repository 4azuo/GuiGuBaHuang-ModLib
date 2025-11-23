# Build script for ModCreator (.NET 9)
param(
    [string]$Configuration = "Release"
)

Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host "ModCreator Build Script (.NET 9)" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is available
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "ERROR: dotnet CLI not found" -ForegroundColor Red
    Write-Host "Please install .NET 9 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Show dotnet version
$dotnetVersion = & dotnet --version
Write-Host "Using .NET SDK: $dotnetVersion" -ForegroundColor Gray
Write-Host ""

$scriptDir = $PSScriptRoot
$projectFile = Join-Path $scriptDir "ModCreator\ModCreator.csproj"
if (-not (Test-Path $projectFile)) {
    Write-Host "ERROR: Project file not found: $projectFile" -ForegroundColor Red
    exit 1
}

# Build project
Write-Host "Building project ($Configuration)..." -ForegroundColor Yellow
Write-Host ""

& dotnet build $projectFile -c $Configuration

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host ""
    
    $outputPath = "ModCreator\bin\$Configuration\net9.0-windows\ModCreator.exe"
    if (Test-Path $outputPath) {
        Write-Host "Output: $outputPath" -ForegroundColor Cyan
        
        # Show file info
        $fileInfo = Get-Item $outputPath
        Write-Host "Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
        Write-Host "Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "To run the application:" -ForegroundColor Yellow
    Write-Host "  .\ModCreator\bin\$Configuration\net9.0-windows\ModCreator.exe" -ForegroundColor White
    Write-Host "  or" -ForegroundColor Gray
    Write-Host "  dotnet run --project ModCreator\ModCreator.csproj -c $Configuration" -ForegroundColor White
    
} else {
    Write-Host ""
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    exit 1
}
