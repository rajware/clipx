#!/bin/sh
# ClipX Uninstallation Script for Linux/macOS
# Usage: ./uninstall.sh [--keep-data]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parse arguments
KEEP_DATA=0
if [ "$1" = "--keep-data" ]; then
    KEEP_DATA=1
fi

echo "${CYAN}ClipX Uninstallation Script${NC}"
echo ""

# Detect platform
OS="$(uname -s)"
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

# Check for root privileges
IS_ROOT=0
if [ "$(id -u)" -eq 0 ]; then
    IS_ROOT=1
fi

# Installation locations
USER_BIN_DIR="$HOME/.local/bin"
USER_MAN_DIR="$HOME/.local/share/man/man1"
USER_COMPLETION_DIR_BASH="$HOME/.local/share/bash-completion/completions"
USER_COMPLETION_DIR_ZSH="$HOME/.zsh/completions"
USER_COMPLETION_DIR_FISH="$HOME/.config/fish/completions"

SYSTEM_BIN_DIR="/usr/local/bin"
SYSTEM_MAN_DIR="/usr/local/share/man/man1"
SYSTEM_COMPLETION_DIR_BASH="/etc/bash_completion.d"
SYSTEM_COMPLETION_DIR_ZSH="/usr/local/share/zsh/site-functions"
SYSTEM_COMPLETION_DIR_FISH="/usr/local/share/fish/vendor_completions.d"

# Data and config locations
if [ "$PLATFORM" = "macos" ]; then
    DATA_DIR="$HOME/Library/Application Support/ClipX"
    CONFIG_DIR="$HOME/Library/Application Support/ClipX"
else
    DATA_DIR="${XDG_DATA_HOME:-$HOME/.local/share}/clipx"
    CONFIG_DIR="${XDG_CONFIG_HOME:-$HOME/.config}/clipx"
fi

REMOVED=0

# Function to remove file if it exists
remove_file() {
    if [ -f "$1" ]; then
        rm -f "$1"
        echo "${GREEN}✓${NC} Removed: $1"
        REMOVED=1
    fi
}

# Function to remove directory if it exists
remove_dir() {
    if [ -d "$1" ]; then
        rm -rf "$1"
        echo "${GREEN}✓${NC} Removed: $1"
        REMOVED=1
    fi
}

# Remove user installation
echo "${CYAN}Checking for user installation...${NC}"
remove_file "$USER_BIN_DIR/clipx"
remove_file "$USER_MAN_DIR/clipx.1"
remove_file "$USER_MAN_DIR/clipx-history.1"
remove_file "$USER_MAN_DIR/clipx-restore.1"
remove_file "$USER_MAN_DIR/clipx-clear.1"
remove_file "$USER_COMPLETION_DIR_BASH/clipx"
remove_file "$USER_COMPLETION_DIR_ZSH/_clipx"
if [ "$PLATFORM" = "linux" ]; then
    remove_file "$USER_COMPLETION_DIR_FISH/clipx.fish"
fi

# Remove system installation (requires root)
if [ -f "$SYSTEM_BIN_DIR/clipx" ] || [ -f "$SYSTEM_MAN_DIR/clipx.1" ]; then
    echo ""
    echo "${CYAN}Checking for system installation...${NC}"
    
    if [ $IS_ROOT -eq 1 ]; then
        remove_file "$SYSTEM_BIN_DIR/clipx"
        remove_file "$SYSTEM_MAN_DIR/clipx.1"
        remove_file "$SYSTEM_MAN_DIR/clipx-history.1"
        remove_file "$SYSTEM_MAN_DIR/clipx-restore.1"
        remove_file "$SYSTEM_MAN_DIR/clipx-clear.1"
        remove_file "$SYSTEM_COMPLETION_DIR_BASH/clipx"
        remove_file "$SYSTEM_COMPLETION_DIR_ZSH/_clipx"
        if [ "$PLATFORM" = "linux" ]; then
            remove_file "$SYSTEM_COMPLETION_DIR_FISH/clipx.fish"
        fi
    else
        echo "${YELLOW}System installation found but requires root privileges to remove${NC}"
        echo "Run with: sudo ./uninstall.sh"
        echo ""
    fi
fi

# Remove data and configuration
if [ $KEEP_DATA -eq 0 ]; then
    if [ -d "$DATA_DIR" ] || [ -d "$CONFIG_DIR" ]; then
        echo ""
        echo "${YELLOW}Warning: This will remove your clipboard history and configuration${NC}"
        echo "Location: $DATA_DIR"
        printf "Are you sure? (y/N): "
        read -r confirm
        
        if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
            remove_dir "$DATA_DIR"
            if [ "$DATA_DIR" != "$CONFIG_DIR" ]; then
                remove_dir "$CONFIG_DIR"
            fi
        else
            echo "${CYAN}Kept data and configuration${NC}"
        fi
    fi
else
    echo ""
    echo "${CYAN}Keeping data and configuration at: $DATA_DIR${NC}"
fi

# Summary
echo ""
if [ $REMOVED -eq 1 ]; then
    echo "${GREEN}✓ ClipX uninstalled successfully${NC}"
    echo ""
    echo "${CYAN}You may need to restart your shell for changes to take effect${NC}"
else
    echo "${YELLOW}No ClipX installation found${NC}"
fi
echo ""
