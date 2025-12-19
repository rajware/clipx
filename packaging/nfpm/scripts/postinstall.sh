#!/bin/sh
# Post-installation script for ClipX packages

set -e

echo "ClipX has been installed successfully!"
echo ""
echo "Usage:"
echo "  clipx copy    - Copy from stdin"
echo "  clipx paste   - Paste to stdout"
echo "  clipx history - View history"
echo "  clipx --help  - Show help"
echo ""
echo "Documentation:"
echo "  man clipx"
echo ""

# Check for clipboard dependencies
if ! command -v xclip >/dev/null 2>&1 && ! command -v wl-copy >/dev/null 2>&1; then
    echo "Note: Install xclip or wl-clipboard for clipboard access:"
    echo "  sudo apt install xclip        # For X11"
    echo "  sudo apt install wl-clipboard # For Wayland"
    echo ""
fi

# Update man database if available
if command -v mandb >/dev/null 2>&1; then
    mandb -q 2>/dev/null || true
fi

exit 0
