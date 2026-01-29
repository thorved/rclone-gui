# Build script for Rclone GUI Installer
# This script builds the application and creates the installer

param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$srcDir = Join-Path $rootDir "src\RcloneGui"
$publishDir = Join-Path $srcDir "bin\x64\Release\net9.0-windows10.0.22621.0\win-x64"
$outputDir = Join-Path $scriptDir "output"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Rclone GUI Installer Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Find MSBuild
$msbuildPath = $null
$possibleMsBuildPaths = @(
    "C:\Program Files\Microsoft Visual Studio\18\*\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\*\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\*\MSBuild\Current\Bin\MSBuild.exe"
)

foreach ($pattern in $possibleMsBuildPaths) {
    $found = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $msbuildPath = $found.FullName
        break
    }
}

# Step 1: Build and publish the application
if (-not $SkipBuild) {
    Write-Host "[1/3] Building application in Release mode..." -ForegroundColor Yellow
    
    if (-not $msbuildPath) {
        Write-Host "  [!] Visual Studio MSBuild not found, falling back to dotnet build..." -ForegroundColor Yellow
        Push-Location $srcDir
        try {
            dotnet build -c Release -p:Platform=x64 -p:TargetFramework=net9.0-windows10.0.22621.0 -p:RuntimeIdentifier=win-x64 -p:SelfContained=true
            if ($LASTEXITCODE -ne 0) {
                throw "Build failed with exit code $LASTEXITCODE"
            }
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Host "  Using MSBuild: $msbuildPath" -ForegroundColor Gray
        & $msbuildPath "$srcDir\RcloneGui.csproj" /p:Configuration=Release /p:Platform=x64 /p:TargetFramework=net9.0-windows10.0.22621.0 /p:RuntimeIdentifier=win-x64 /p:SelfContained=true /t:Rebuild /v:m
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }
    }
    
    Write-Host "  Application built to: $publishDir" -ForegroundColor Green
}
else {
    Write-Host "[1/3] Skipping build (using existing output)..." -ForegroundColor Gray
}

# Step 2: Verify prerequisites
Write-Host ""
Write-Host "[2/3] Verifying prerequisites..." -ForegroundColor Yellow

# Check build output exists
if (-not (Test-Path (Join-Path $publishDir "RcloneGui.exe"))) {
    throw "Built application not found at: $publishDir. Run without -SkipBuild first."
}
Write-Host "  [OK] Built application found" -ForegroundColor Green

# Check WinFsp MSI exists
$winfspMsi = Join-Path $rootDir "binary\winfsp-2.0.23075.msi"
if (-not (Test-Path $winfspMsi)) {
    throw "WinFsp MSI not found at: $winfspMsi"
}
Write-Host "  [OK] WinFsp MSI found" -ForegroundColor Green

# Check rclone.exe exists
$rcloneExe = Join-Path $rootDir "binary\rclone-v1.72.1\rclone.exe"
if (-not (Test-Path $rcloneExe)) {
    throw "rclone.exe not found at: $rcloneExe"
}
Write-Host "  [OK] rclone.exe found" -ForegroundColor Green

# Check Inno Setup is installed
$innoSetupPath = $null
$possiblePaths = @(
    "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $innoSetupPath = $path
        break
    }
}

if (-not $innoSetupPath) {
    Write-Host ""
    Write-Host "  [!] Inno Setup 6 not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Please install Inno Setup 6 from:" -ForegroundColor Yellow
    Write-Host "  https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Or install via winget:" -ForegroundColor Yellow
    Write-Host "  winget install JRSoftware.InnoSetup" -ForegroundColor Cyan
    Write-Host ""
    throw "Inno Setup 6 is required to build the installer"
}
Write-Host "  [OK] Inno Setup found at: $innoSetupPath" -ForegroundColor Green

# Step 3: Build the installer
if (-not $SkipInstaller) {
    Write-Host ""
    Write-Host "[3/3] Building installer..." -ForegroundColor Yellow
    
    # Create output directory
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir | Out-Null
    }
    
    $issFile = Join-Path $scriptDir "RcloneGuiSetup.iss"
    
    Write-Host "  Running Inno Setup Compiler..."
    & $innoSetupPath $issFile
    
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed with exit code $LASTEXITCODE"
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Installer created at:" -ForegroundColor White
    Get-ChildItem $outputDir -Filter "*.exe" | ForEach-Object {
        Write-Host "  $($_.FullName)" -ForegroundColor Cyan
    }
    Write-Host ""
}
else {
    Write-Host "[3/3] Skipping installer build..." -ForegroundColor Gray
}
