#!/bin/sh
# Pre-removal script for ClipX packages

set -e

echo "Removing ClipX..."
echo "Your clipboard history and configuration will be preserved in:"
echo "  ~/.local/share/clipx"
echo "  ~/.config/clipx"
echo ""
echo "To remove data, run:"
echo "  rm -rf ~/.local/share/clipx ~/.config/clipx"
echo ""

exit 0
