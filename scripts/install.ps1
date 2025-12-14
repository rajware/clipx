# ClipX Installation Script for Windows
# Usage: .\install.ps1 [-User] [-System]
# Run as Administrator for system-wide installation

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$User,
    
    [Parameter()]
    [switch]$System
)

# Colors for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }

# Determine installation mode
$InstallMode = "user"
if ($System) {
    $InstallMode = "system"
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $isAdmin) {
        Write-Error "Error: -System requires Administrator privileges"
        Write-Host "Run PowerShell as Administrator and try again"
        exit 1
    }
}

Write-Success "ClipX Installation Script for Windows"
Write-Host "Mode: $InstallMode"
Write-Host ""

# Detect architecture
$arch = if ([Environment]::Is64BitOperatingSystem) { "x64" } else { "x86" }
$runtimeId = "win-$arch"

Write-Info "Platform: $runtimeId"
Write-Host ""

# Set installation directories
if ($InstallMode -eq "system") {
    $binDir = "$env:ProgramFiles\ClipX"
    $addToPath = $true
} else {
    $binDir = "$env:LOCALAPPDATA\Programs\ClipX"
    $addToPath = $true
}

# Check if binary exists
$binaryPath = "publish\$runtimeId\clipx.exe"
if (-not (Test-Path $binaryPath)) {
    Write-Warning "Binary not found at $binaryPath"
    Write-Info "Building for $runtimeId..."
    
    # Check if dotnet is available
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error "Error: .NET SDK not found"
        Write-Host "Install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    }
    
    # Build the project
    dotnet publish src\ClipX.CLI\ClipX.CLI.csproj `
        -c Release `
        -r $runtimeId `
        --self-contained `
        -p:PublishSingleFile=true `
        -o "publish\$runtimeId"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
}

# Create installation directory
Write-Info "Creating directory: $binDir"
New-Item -ItemType Directory -Path $binDir -Force | Out-Null

# Copy binary
Write-Info "Installing binary to $binDir..."
Copy-Item $binaryPath "$binDir\clipx.exe" -Force

# Install PowerShell completion
$profileDir = Split-Path $PROFILE -Parent
$completionPath = "$profileDir\clipx-completion.ps1"

Write-Info "Installing PowerShell completion..."
New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
Copy-Item "completions\powershell\clipx-completion.ps1" $completionPath -Force

# Add to PATH if needed
if ($addToPath) {
    $pathScope = if ($InstallMode -eq "system") { "Machine" } else { "User" }
    $currentPath = [Environment]::GetEnvironmentVariable("Path", $pathScope)
    
    if ($currentPath -notlike "*$binDir*") {
        Write-Info "Adding $binDir to PATH..."
        $newPath = "$binDir;$currentPath"
        [Environment]::SetEnvironmentVariable("Path", $newPath, $pathScope)
        
        # Update current session PATH
        $env:Path = "$binDir;$env:Path"
        
        Write-Success "✓ Added to PATH"
    } else {
        Write-Info "Already in PATH"
    }
}

# Add completion to PowerShell profile
if (Test-Path $PROFILE) {
    $profileContent = Get-Content $PROFILE -Raw
    $completionLine = ". `"$completionPath`""
    
    if ($profileContent -notlike "*clipx-completion.ps1*") {
        Write-Info "Adding completion to PowerShell profile..."
        Add-Content $PROFILE "`n# ClipX completion`n$completionLine"
        Write-Success "✓ Added to profile"
    } else {
        Write-Info "Completion already in profile"
    }
} else {
    Write-Warning "PowerShell profile not found at: $PROFILE"
    Write-Host "To enable completions, add this to your profile:"
    Write-Host "  . `"$completionPath`""
    Write-Host ""
    Write-Host "Create profile with:"
    Write-Host "  New-Item -Path $PROFILE -ItemType File -Force"
}

# Success message
Write-Host ""
Write-Success "✓ ClipX installed successfully!"
Write-Host ""
Write-Host "Installation location: $binDir"
Write-Host ""
Write-Host "Usage:"
Write-Host "  clipx copy    - Copy from stdin"
Write-Host "  clipx paste   - Paste to stdout"
Write-Host "  clipx history - View history"
Write-Host "  clipx --help  - Show help"
Write-Host ""

# Verify installation
Write-Info "Verifying installation..."
$clipxPath = Get-Command clipx -ErrorAction SilentlyContinue
if ($clipxPath) {
    Write-Success "✓ clipx is in PATH and ready to use"
    Write-Host ""
    Write-Host "Try it:"
    Write-Host '  "Hello from Windows" | clipx copy'
    Write-Host "  clipx paste"
} else {
    Write-Warning "Note: You may need to restart PowerShell for PATH changes to take effect"
    Write-Host ""
    Write-Host "After restarting, verify with:"
    Write-Host "  clipx --version"
}

Write-Host ""
Write-Info "To enable tab completion, restart PowerShell or run:"
Write-Host "  . $PROFILE"
Write-Host ""

# Data locations
Write-Host "Data and configuration will be stored in:"
Write-Host "  $env:LOCALAPPDATA\ClipX"
Write-Host ""
