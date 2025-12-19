<#
.SYNOPSIS
Builds the NSIS installer for ClipX.

.DESCRIPTION
This script compiles the NSIS script to create a Windows installer executable.
It requires NSIS (makensis) to be installed and available in the PATH.
#>


param(
    [string]$Version = "0.0.0-dev"
)

$ErrorActionPreference = "Stop"

Write-Host "Building ClipX v$Version NSIS installer..."
Write-Host ""

# ... (omitted checks)

# Build installer
Write-Host "Compiling NSIS script..."
Push-Location $PSScriptRoot

try {
    makensis -DPRODUCT_VERSION="$Version" clipx.nsi
    if ($LASTEXITCODE -ne 0) {
        throw "NSIS compilation failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "✓ Installer created: dist\clipx-setup-$Version.exe" -ForegroundColor Green
Write-Host ""
Write-Host "Test with:"
Write-Host "  dist\clipx-setup-$Version.exe"

Write-Host ""
