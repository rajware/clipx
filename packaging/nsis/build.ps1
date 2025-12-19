<#
.SYNOPSIS
Builds the NSIS installer for ClipX.

.DESCRIPTION
This script compiles the NSIS script to create a Windows installer executable.
It requires NSIS (makensis) to be installed and available in the PATH.
#>

$ErrorActionPreference = "Stop"

Write-Host "Building ClipX NSIS installer..."
Write-Host ""

# Check for makensis
if (-not (Get-Command "makensis" -ErrorAction SilentlyContinue)) {
    Write-Host "Error: NSIS (makensis) not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Install NSIS:"
    Write-Host "  Download from https://nsis.sourceforge.io/"
    Write-Host "  Or use Chocolatey: choco install nsis"
    Write-Host "  Or use Scoop: scoop install nsis"
    exit 1
}

# Ensure binary exists
$BinaryPath =Join-Path $PSScriptRoot "..\..\publish\win-x64\clipx.exe"
if (-not (Test-Path $BinaryPath)) {
    Write-Host "Error: Windows binary not found at $BinaryPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Build it first with:"
    Write-Host "  dotnet publish src/ClipX.CLI/ClipX.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish/win-x64"
    exit 1
}

# Create output directory
$DistDir = Join-Path $PSScriptRoot "..\..\dist"
if (-not (Test-Path $DistDir)) {
    New-Item -ItemType Directory -Path $DistDir | Out-Null
}

# Build installer
Write-Host "Compiling NSIS script..."
Push-Location $PSScriptRoot

try {
    makensis clipx.nsi
    if ($LASTEXITCODE -ne 0) {
        throw "NSIS compilation failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "✓ Installer created: dist\clipx-setup-0.2.0.exe" -ForegroundColor Green
Write-Host ""
Write-Host "Test with:"
Write-Host "  dist\clipx-setup-0.2.0.exe"
Write-Host ""
