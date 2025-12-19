#!/bin/bash
# Build NSIS installer
# Can be run on Linux or Windows

set -e


VERSION="${1:-0.0.0-dev}"
echo "Building ClipX v${VERSION} NSIS installer..."

# ... (omitted checks)

makensis -DPRODUCT_VERSION="${VERSION}" clipx.nsi

# ...

echo "✓ Installer created: dist/clipx-setup-${VERSION}.exe"
echo ""
echo "Test with:"
echo "  dist/clipx-setup-${VERSION}.exe"

echo ""
