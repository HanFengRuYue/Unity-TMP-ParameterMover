# Unity TMP Parameter Mover - Build Script
# This script compiles and publishes the application to the Release folder

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [string]$OutputFolder = "Release"
)

# Set error handling
$ErrorActionPreference = "Stop"

# Get script directory (project root)
$RootDir = $PSScriptRoot
$ProjectDir = Join-Path $RootDir "Unity-TMP-ParameterMover-WinUI"
$ProjectFile = Join-Path $ProjectDir "Unity-TMP-ParameterMover-WinUI.csproj"
$OutputDir = Join-Path $RootDir $OutputFolder

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Unity TMP Parameter Mover - Build Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if project file exists
if (-not (Test-Path $ProjectFile)) {
    Write-Host "Error: Project file not found: $ProjectFile" -ForegroundColor Red
    exit 1
}

# Step 1: Clean previous build
Write-Host "[1/4] Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
    Write-Host "Removed old output folder" -ForegroundColor Green
}

# Clean project build cache
Set-Location $ProjectDir
dotnet clean --configuration $Configuration
Write-Host "Clean completed" -ForegroundColor Green
Write-Host ""

# Step 2: Restore NuGet packages
Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Restore completed" -ForegroundColor Green
Write-Host ""

# Step 3: Build project
Write-Host "[3/4] Building project ($Configuration | $Platform)..." -ForegroundColor Yellow
dotnet build --configuration $Configuration -p:Platform=$Platform --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build completed" -ForegroundColor Green
Write-Host ""

# Step 4: Publish application
Write-Host "[4/4] Publishing application (framework-dependent)..." -ForegroundColor Yellow

# Set runtime identifier
$RuntimeId = "win-$Platform"

# Publish as framework-dependent single file
dotnet publish `
    --configuration $Configuration `
    --runtime $RuntimeId `
    --self-contained false `
    --output $OutputDir `
    -p:Platform=$Platform `
    -p:PublishSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Publish completed" -ForegroundColor Green
Write-Host ""

# Remove PDB files (debug symbols) to reduce size
Write-Host "Cleaning up debug files..." -ForegroundColor Yellow
Get-ChildItem -Path $OutputDir -Filter "*.pdb" -Recurse | Remove-Item -Force
Write-Host "Cleanup completed" -ForegroundColor Green
Write-Host ""

# Display output information
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output location: $OutputDir" -ForegroundColor White
Write-Host "Executable file: Unity_TMP_ParameterMover_WinUI.exe" -ForegroundColor White
Write-Host ""

# Get output folder size
$FolderSize = (Get-ChildItem -Path $OutputDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Output folder size: $([math]::Round($FolderSize, 2)) MB" -ForegroundColor Cyan

# Count files
$FileCount = (Get-ChildItem -Path $OutputDir -File -Recurse).Count
Write-Host "Total files: $FileCount" -ForegroundColor Cyan
Write-Host ""

# Ask to open output folder
$Response = Read-Host "Open output folder? (Y/N)"
if ($Response -eq "Y" -or $Response -eq "y") {
    Start-Process explorer.exe -ArgumentList $OutputDir
}

Write-Host ""
Write-Host "Build script completed!" -ForegroundColor Green
