#!/bin/bash
# Build NSIS installer
# Can be run on Linux or Windows

set -e

VERSION="${1:-0.0.0-dev}"
echo "Building ClipX v${VERSION} NSIS installer..."
echo ""

# Check if makensis is installed
if ! command -v makensis >/dev/null 2>&1; then
    echo "Error: NSIS (makensis) not found"
    echo ""
    echo "Install NSIS:"
    echo "  Linux:   sudo apt install nsis"
    echo "  macOS:   brew install makensis"
    echo "  Windows: Download from https://nsis.sourceforge.io/"
    exit 1
fi

# Ensure binary exists
if [ ! -f "publish/win-x64/clipx.exe" ]; then
    echo "Error: Windows binary not found at publish/win-x64/clipx.exe"
    echo ""
    echo "Build it first with:"
    echo "  dotnet publish src/ClipX.CLI/ClipX.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish/win-x64"
    exit 1
fi

# Create output directory
mkdir -p dist

# Build installer
echo "Compiling NSIS script..."
cd packaging/nsis

makensis -DPRODUCT_VERSION="${VERSION}" clipx.nsi

cd ../..

echo ""
echo "✓ Installer created: dist/clipx-setup-${VERSION}.exe"
echo ""
echo "Test with:"
echo "  dist/clipx-setup-${VERSION}.exe"
echo ""
