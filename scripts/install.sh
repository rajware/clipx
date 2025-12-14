#!/bin/sh
# ClipX Installation Script for Linux/macOS
# Usage: ./install.sh [--user|--system]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Detect installation mode
INSTALL_MODE="user"
if [ "$1" = "--system" ]; then
    INSTALL_MODE="system"
    if [ "$(id -u)" -ne 0 ]; then
        echo "${RED}Error: --system requires root privileges${NC}"
        echo "Run with: sudo ./install.sh --system"
        exit 1
    fi
fi

# Detect platform
OS="$(uname -s)"
ARCH="$(uname -m)"

case "$OS" in
    Linux*)
        PLATFORM="linux"
        ;;
    Darwin*)
        PLATFORM="macos"
        ;;
    *)
        echo "${RED}Unsupported OS: $OS${NC}"
        exit 1
        ;;
esac

# Map architecture
case "$ARCH" in
    x86_64|amd64)
        ARCH_NAME="x64"
        ;;
    aarch64|arm64)
        ARCH_NAME="arm64"
        ;;
    *)
        echo "${RED}Unsupported architecture: $ARCH${NC}"
        exit 1
        ;;
esac

RUNTIME_ID="${PLATFORM}-${ARCH_NAME}"

echo "${GREEN}ClipX Installation Script${NC}"
echo "Platform: $RUNTIME_ID"
echo "Mode: $INSTALL_MODE"
echo ""

# Set installation directories
if [ "$INSTALL_MODE" = "system" ]; then
    BIN_DIR="/usr/local/bin"
    MAN_DIR="/usr/local/share/man/man1"
    COMPLETION_DIR_BASH="/etc/bash_completion.d"
    COMPLETION_DIR_ZSH="/usr/local/share/zsh/site-functions"
    COMPLETION_DIR_FISH="/usr/local/share/fish/vendor_completions.d"
else
    BIN_DIR="$HOME/.local/bin"
    MAN_DIR="$HOME/.local/share/man/man1"
    COMPLETION_DIR_BASH="$HOME/.local/share/bash-completion/completions"
    COMPLETION_DIR_ZSH="$HOME/.zsh/completions"
    COMPLETION_DIR_FISH="$HOME/.config/fish/completions"
fi

# Check if binary exists
BINARY="publish/$RUNTIME_ID/clipx"
if [ ! -f "$BINARY" ]; then
    echo "${YELLOW}Binary not found at $BINARY${NC}"
    echo "Building for $RUNTIME_ID..."
    dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
        -c Release \
        -r "$RUNTIME_ID" \
        --self-contained \
        -p:PublishSingleFile=true \
        -o "publish/$RUNTIME_ID"
fi

# Create directories
echo "Creating directories..."
mkdir -p "$BIN_DIR"
mkdir -p "$MAN_DIR"
mkdir -p "$COMPLETION_DIR_BASH"
mkdir -p "$COMPLETION_DIR_ZSH"
if [ "$PLATFORM" = "linux" ]; then
    mkdir -p "$COMPLETION_DIR_FISH"
fi

# Install binary
echo "Installing binary to $BIN_DIR..."
cp "$BINARY" "$BIN_DIR/clipx"
chmod +x "$BIN_DIR/clipx"

# Install man pages
echo "Installing man pages to $MAN_DIR..."
cp docs/man/*.1 "$MAN_DIR/"

# Install completions
echo "Installing shell completions..."
cp completions/bash/clipx "$COMPLETION_DIR_BASH/clipx"
cp completions/zsh/_clipx "$COMPLETION_DIR_ZSH/_clipx"
if [ "$PLATFORM" = "linux" ]; then
    cp completions/fish/clipx.fish "$COMPLETION_DIR_FISH/clipx.fish"
fi

# Check if binary directory is in PATH
if ! echo "$PATH" | grep -q "$BIN_DIR"; then
    echo ""
    echo "${YELLOW}Warning: $BIN_DIR is not in your PATH${NC}"
    echo "Add it to your shell profile:"
    echo ""
    if [ "$PLATFORM" = "macos" ]; then
        echo "  echo 'export PATH=\"$BIN_DIR:\$PATH\"' >> ~/.zshrc"
    else
        echo "  echo 'export PATH=\"$BIN_DIR:\$PATH\"' >> ~/.bashrc"
    fi
    echo ""
fi

# Success message
echo ""
echo "${GREEN}✓ ClipX installed successfully!${NC}"
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

# Platform-specific notes
if [ "$PLATFORM" = "linux" ]; then
    if ! command -v xclip >/dev/null 2>&1 && ! command -v wl-copy >/dev/null 2>&1; then
        echo "${YELLOW}Note: Install xclip or wl-clipboard for clipboard access:${NC}"
        echo "  sudo apt install xclip        # For X11"
        echo "  sudo apt install wl-clipboard # For Wayland"
        echo ""
    fi
fi

# Completion activation instructions
echo "To activate completions, restart your shell or run:"
if [ "$PLATFORM" = "macos" ]; then
    echo "  source ~/.zshrc"
else
    echo "  source ~/.bashrc"
fi
