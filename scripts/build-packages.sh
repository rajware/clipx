#!/bin/bash
# Build all ClipX packages for distribution
# This script builds binaries and creates packages for all platforms

set -e

VERSION="${1:-0.0.0-dev}"
echo "Building ClipX v${VERSION} packages..."
echo ""

# Create output directories
mkdir -p dist/packages dist/tarballs

# Build binaries for all platforms
echo "==> Building binaries..."
echo ""

echo "Building Linux x64..."
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:Version=${VERSION} \
  -o ./publish/linux-x64
cp ./publish/linux-x64/clipx ./publish/linux-x64/clipx-linux-x64

echo "Building macOS x64..."
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:Version=${VERSION} \
  -o ./publish/osx-x64
cp ./publish/osx-x64/clipx ./publish/osx-x64/clipx-osx-x64

echo "Building macOS ARM64..."
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:Version=${VERSION} \
  -o ./publish/osx-arm64
cp ./publish/osx-arm64/clipx ./publish/osx-arm64/clipx-osx-arm64

echo "Building Windows x64..."

dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:Version=${VERSION} \
  -o ./publish/win-x64
cp ./publish/win-x64/clipx.exe ./publish/win-x64/clipx-win-x64.exe

echo ""
echo "==> Creating macOS tarballs..."
echo ""

# Create macOS tarballs for Homebrew
(
  cd publish/osx-x64
  tar -czf ../../dist/tarballs/clipx-macos-x64.tar.gz \
    clipx \
    ../../docs/man/*.1 \
    ../../completions/bash/clipx \
    ../../completions/zsh/_clipx
)

(
  cd publish/osx-arm64
  tar -czf ../../dist/tarballs/clipx-macos-arm64.tar.gz \
    clipx \
    ../../docs/man/*.1 \
    ../../completions/bash/clipx \
    ../../completions/zsh/_clipx
)

# Calculate checksums
# Calculate checksums and update Homebrew formula
echo "Calculating checksums..."
HASH_X64=$(shasum -a 256 dist/tarballs/clipx-macos-x64.tar.gz | cut -d ' ' -f 1)
HASH_ARM64=$(shasum -a 256 dist/tarballs/clipx-macos-arm64.tar.gz | cut -d ' ' -f 1)

echo "macOS x64 SHA256: ${HASH_X64}"
echo "macOS ARM64 SHA256: ${HASH_ARM64}"
echo ""

TEMPLATE_FILE="packaging/homebrew/clipx.rb"
FORMULA_DIR="dist/homebrew/Formula"
FORMULA_FILE="${FORMULA_DIR}/clipx.rb"

mkdir -p "$FORMULA_DIR"

if [ -f "$TEMPLATE_FILE" ]; then
    echo "Generating Homebrew formula..."
    
    # Read template and replace placeholders
    # We use sed for simple replacements
    sed "s|__VERSION__|${VERSION}|g" "$TEMPLATE_FILE" > "$FORMULA_FILE"
    
    # In-place replacements for hashes
    # We use perl for safer file editing (cross-platform sed is tricky with in-place)
    perl -i -pe "s|__SHA256_X64__|${HASH_X64}|g" "$FORMULA_FILE"
    perl -i -pe "s|__SHA256_ARM64__|${HASH_ARM64}|g" "$FORMULA_FILE"
    
    echo "Created $FORMULA_FILE"
fi
echo ""

# Build Linux packages with nfpm
if command -v nfpm >/dev/null 2>&1; then
    echo "==> Building Linux packages with nfpm..."
    echo ""
    
    for packager in deb rpm apk archlinux; do
        echo "Building ${packager} package..."
        nfpm pkg --packager ${packager} --config packaging/nfpm/nfpm.yaml --target dist/packages/
    done
else
    echo "Warning: nfpm not found. Skipping Linux packages."
    echo "Install with: go install github.com/goreleaser/nfpm/v2/cmd/nfpm@latest"
fi

# Build Windows installer with NSIS
if command -v makensis >/dev/null 2>&1; then
    echo ""
    echo "==> Building Windows installer with NSIS..."
    echo ""
    ./packaging/nsis/build.sh
else
    echo ""
    echo "Warning: NSIS (makensis) not found. Skipping Windows installer."
    echo "Install with: sudo apt install nsis (Linux) or brew install makensis (macOS)"
fi

echo ""
echo "==> Build complete!"
echo ""
echo "Packages created:"
echo "  Windows:  dist/clipx-setup-${VERSION}.exe"
echo "  macOS:    dist/tarballs/clipx-macos-*.tar.gz"
echo "  DEB:      dist/packages/clipx_${VERSION}_amd64.deb"
echo "  RPM:      dist/packages/clipx-${VERSION}.x86_64.rpm"
echo "  APK:      dist/packages/clipx-${VERSION}.apk"
echo "  Arch:     dist/packages/clipx-${VERSION}-1-x86_64.pkg.tar.zst"
echo ""
echo "Next steps:"
echo "  1. Update Homebrew formula with new SHA256 checksums"
echo "  2. Test packages on target systems"
echo "  3. Upload to GitHub releases"
