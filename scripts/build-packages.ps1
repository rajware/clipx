<#
.SYNOPSIS
Builds all ClipX packages for distribution on Windows.

.DESCRIPTION
This script builds binaries for Windows, Linux, and macOS, and creates distribution packages.
It requires:
- .NET 8.0 SDK
- tar (for macOS tarballs)
- makensis (for Windows installer)
- nfpm (optional, for Linux packages)
#>


param(
    [string]$Version = "0.0.0-dev"
)

$ErrorActionPreference = "Stop"

Write-Host "Building ClipX v$Version packages..."

Write-Host ""

$ScriptDir = $PSScriptRoot
$RootDir = Join-Path $ScriptDir ".."

# Helper to check command availability
function Test-Command {
    param($Name)
    if (Get-Command $Name -ErrorAction SilentlyContinue) { return $true }
    return $false
}

# Create output directories
$DistDir = Join-Path $RootDir "dist"
$PackagesDir = Join-Path $DistDir "packages"
$TarballsDir = Join-Path $DistDir "tarballs"

New-Item -ItemType Directory -Force -Path $PackagesDir | Out-Null
New-Item -ItemType Directory -Force -Path $TarballsDir | Out-Null

# Build binaries
Write-Host "==> Building binaries..."
Write-Host ""

$Platforms = @(
    @{ Name="Linux x64"; Rid="linux-x64" },
    @{ Name="macOS x64"; Rid="osx-x64" },
    @{ Name="macOS ARM64"; Rid="osx-arm64" },
    @{ Name="Windows x64"; Rid="win-x64" }
)

foreach ($p in $Platforms) {
    Write-Host "Building $($p.Name)..."
    $OutputDir = Join-Path $RootDir "publish\$($p.Rid)"
    $ProjectFile = Join-Path $RootDir "src" "ClipX.CLI" "ClipX.CLI.csproj"
    dotnet publish $ProjectFile `
        -c Release `
        -r $p.Rid `
        --self-contained `
        -p:PublishSingleFile=true `
        -p:Version=$Version `
        -o $OutputDir
}

Write-Host ""
Write-Host "==> Creating macOS tarballs..."
Write-Host ""

if (Test-Command "tar") {
    # macOS x64
    Write-Host "Creating macos-x64 tarball..."
    Push-Location (Join-Path $RootDir "publish\osx-x64")
    try {
        $TarballPath = Join-Path $RootDir "dist" "tarballs" "clipx-macos-x64.tar.gz"
        $ManPageParams = @(
            Get-ChildItem (Join-Path $RootDir "docs" "man" "*.1") | ForEach-Object { "../../docs/man/" + $_.Name }
        )
        
        tar -czf $TarballPath "clipx" "../../completions/bash/clipx" "../../completions/zsh/_clipx" $ManPageParams
    }
    finally { Pop-Location }

    # macOS ARM64
    Write-Host "Creating macos-arm64 tarball..."
    Push-Location (Join-Path $RootDir "publish\osx-arm64")
    try {
        $TarballPath = Join-Path $RootDir "dist" "tarballs" "clipx-macos-arm64.tar.gz"
        $ManPageParams = @(
            Get-ChildItem (Join-Path $RootDir "docs" "man" "*.1") | ForEach-Object { "../../docs/man/" + $_.Name }
        )

        tar -czf $TarballPath "clipx" "../../completions/bash/clipx" "../../completions/zsh/_clipx" $ManPageParams
    }
    finally { Pop-Location }
}
else {
    Write-Warning "tar command not found. Skipping macOS tarball creation."
}

# Checksums
Write-Host ""
Write-Host "Calculating checksums..."
    $HashX64 = ""
    $HashArm64 = ""

    $TarballX64 = Join-Path $TarballsDir "clipx-macos-x64.tar.gz"
    if (Test-Path $TarballX64) {
        $HashX64 = (Get-FileHash $TarballX64 -Algorithm SHA256).Hash.ToLower()
        Write-Host "macOS x64 SHA256: $HashX64"
    }

    $TarballArm64 = Join-Path $TarballsDir "clipx-macos-arm64.tar.gz"
    if (Test-Path $TarballArm64) {
        $HashArm64 = (Get-FileHash $TarballArm64 -Algorithm SHA256).Hash.ToLower()
        Write-Host "macOS ARM64 SHA256: $HashArm64"
    }

    # Generate Homebrew formula
    $TemplatePath = Join-Path $RootDir "packaging" "homebrew" "clipx.rb"
    $FormulaDir = Join-Path $DistDir "homebrew" "Formula"
    
    if (-not (Test-Path $FormulaDir)) {
        New-Item -ItemType Directory -Force -Path $FormulaDir | Out-Null
    }

    $FormulaPath = Join-Path $FormulaDir "clipx.rb"

    if (Test-Path $TemplatePath) {
        Write-Host "Generating Homebrew formula..."
        $Content = Get-Content $TemplatePath -Raw
        
        # Replace placeholders
        $Content = $Content.Replace("__VERSION__", $Version)
        
        if ($HashArm64) {
            $Content = $Content.Replace("__SHA256_ARM64__", $HashArm64)
        }
        
        if ($HashX64) {
            $Content = $Content.Replace("__SHA256_X64__", $HashX64)
        }

        Set-Content -Path $FormulaPath -Value $Content -NoNewline
        Write-Host "Created $FormulaPath"
    }


# Windows Installer
Write-Host ""
Write-Host "==> Building Windows installer with NSIS..."
if (Test-Command "makensis") {
    try {
        & "$RootDir\packaging\nsis\build.ps1"
    }
    catch {
        Write-Error "Failed to build Windows installer: $_"
    }
}
else {
    Write-Warning "NSIS (makensis) not found. Skipping Windows installer. Install from https://nsis.sourceforge.io/"
}

# Linux Packages (nfpm)
Write-Host ""
if (Test-Command "nfpm") {
    Write-Host "==> Building Linux packages with nfpm..."
    $Packagers = "deb", "rpm", "apk", "archlinux"
    foreach ($packager in $Packagers) {
        Write-Host "Building $packager package..."
        $ConfigPath = Join-Path $RootDir "packaging" "nfpm" "nfpm.yaml"
        $TargetDir = Join-Path $DistDir "packages"
        nfpm pkg --packager $packager --config $ConfigPath --target $TargetDir
    }
}
else {
    Write-Warning "nfpm not found. Skipping Linux packages. (Use 'go install github.com/goreleaser/nfpm/v2/cmd/nfpm@latest' to install)"
}

Write-Host ""
Write-Host "==> Build complete!" -ForegroundColor Green
Write-Host ""
