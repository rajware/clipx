# ClipX Uninstallation Script for Windows
# Usage: .\uninstall.ps1 [-KeepData]

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$KeepData
)

function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }

Write-Info "ClipX Uninstallation Script for Windows"
Write-Host ""

# Check for admin privileges
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

# Installation locations
$userBinDir = "$env:LOCALAPPDATA\Programs\ClipX"
$systemBinDir = "$env:ProgramFiles\ClipX"
$dataDir = "$env:LOCALAPPDATA\ClipX"
$configDir = "$env:LOCALAPPDATA\ClipX"
$profileDir = Split-Path $PROFILE -Parent
$completionPath = "$profileDir\clipx-completion.ps1"

$removed = $false

# Remove user installation
if (Test-Path $userBinDir) {
    Write-Info "Removing user installation from $userBinDir..."
    Remove-Item $userBinDir -Recurse -Force
    Write-Success "✓ Removed user installation"
    $removed = $true
    
    # Remove from user PATH
    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($userPath -like "*$userBinDir*") {
        Write-Info "Removing from user PATH..."
        $newPath = ($userPath -split ';' | Where-Object { $_ -ne $userBinDir }) -join ';'
        [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
        Write-Success "✓ Removed from PATH"
    }
}

# Remove system installation (requires admin)
if (Test-Path $systemBinDir) {
    if ($isAdmin) {
        Write-Info "Removing system installation from $systemBinDir..."
        Remove-Item $systemBinDir -Recurse -Force
        Write-Success "✓ Removed system installation"
        $removed = $true
        
        # Remove from system PATH
        $systemPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
        if ($systemPath -like "*$systemBinDir*") {
            Write-Info "Removing from system PATH..."
            $newPath = ($systemPath -split ';' | Where-Object { $_ -ne $systemBinDir }) -join ';'
            [Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
            Write-Success "✓ Removed from PATH"
        }
    } else {
        Write-Warning "System installation found but requires Administrator privileges to remove"
        Write-Host "Run as Administrator to remove system installation"
    }
}

# Remove PowerShell completion
if (Test-Path $completionPath) {
    Write-Info "Removing PowerShell completion..."
    Remove-Item $completionPath -Force
    Write-Success "✓ Removed completion script"
}

# Remove from PowerShell profile
if (Test-Path $PROFILE) {
    $profileContent = Get-Content $PROFILE -Raw
    if ($profileContent -like "*clipx-completion.ps1*") {
        Write-Info "Removing from PowerShell profile..."
        $newContent = $profileContent -replace "(?m)^\s*#\s*ClipX completion\s*$", "" `
                                       -replace "(?m)^\s*\.\s*`".*clipx-completion\.ps1`"\s*$", ""
        $newContent = $newContent -replace "(\r?\n){3,}", "`r`n`r`n"  # Remove extra blank lines
        Set-Content $PROFILE $newContent.TrimEnd()
        Write-Success "✓ Removed from profile"
    }
}

# Remove data and configuration
if (-not $KeepData) {
    if (Test-Path $dataDir) {
        Write-Warning "Removing clipboard history and configuration..."
        Write-Host "Location: $dataDir"
        
        $confirm = Read-Host "Are you sure? (y/N)"
        if ($confirm -eq 'y' -or $confirm -eq 'Y') {
            Remove-Item $dataDir -Recurse -Force
            Write-Success "✓ Removed data and configuration"
        } else {
            Write-Info "Kept data and configuration"
        }
    }
} else {
    Write-Info "Keeping data and configuration at: $dataDir"
}

# Summary
Write-Host ""
if ($removed) {
    Write-Success "✓ ClipX uninstalled successfully"
    Write-Host ""
    Write-Info "You may need to restart PowerShell for PATH changes to take effect"
} else {
    Write-Warning "No ClipX installation found"
}
Write-Host ""
