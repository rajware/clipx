# ClipX Installation Guide

## Quick Install

### Linux/macOS

**User installation** (recommended):
```bash
./scripts/install.sh
```

**System-wide installation**:
```bash
sudo ./scripts/install.sh --system
```

### Windows

**User installation** (recommended):
```powershell
.\scripts\install.ps1
```

**System-wide installation** (run PowerShell as Administrator):
```powershell
.\scripts\install.ps1 -System
```

**For detailed Windows instructions**, see [WINDOWS_INSTALLATION.md](WINDOWS_INSTALLATION.md)

---

## Platform-Specific Instructions

### Linux

#### Prerequisites
Install clipboard tool (choose based on your display server):

**X11** (most desktop environments):
```bash
sudo apt install xclip        # Debian/Ubuntu
sudo dnf install xclip        # Fedora
sudo pacman -S xclip          # Arch
```

**Wayland** (GNOME on Wayland, Sway, etc.):
```bash
sudo apt install wl-clipboard # Debian/Ubuntu
sudo dnf install wl-clipboard # Fedora
sudo pacman -S wl-clipboard   # Arch
```

#### Installation Locations

**User installation** (`./scripts/install.sh`):
- Binary: `~/.local/bin/clipx`
- Man pages: `~/.local/share/man/man1/`
- Bash completion: `~/.local/share/bash-completion/completions/`
- Zsh completion: `~/.zsh/completions/`
- Fish completion: `~/.config/fish/completions/`

**System installation** (`sudo ./scripts/install.sh --system`):
- Binary: `/usr/local/bin/clipx`
- Man pages: `/usr/local/share/man/man1/`
- Bash completion: `/etc/bash_completion.d/`
- Zsh completion: `/usr/local/share/zsh/site-functions/`
- Fish completion: `/usr/local/share/fish/vendor_completions.d/`

#### Shell Completions

**Bash**:
Completions are installed automatically. Restart your shell:
```bash
source ~/.bashrc
```

**Zsh**:
Add to `~/.zshrc` if using user installation:
```bash
fpath=(~/.zsh/completions $fpath)
autoload -Uz compinit && compinit
```

**Fish**:
Completions work automatically after restart:
```bash
exec fish
```

---

### macOS

#### Prerequisites
No additional dependencies required. macOS includes `pbcopy` and `pbpaste`.

#### Installation

**Homebrew** (coming soon):
```bash
brew install clipx
```

**Manual installation**:
```bash
./scripts/install.sh
```

#### Installation Locations

Same as Linux user installation:
- Binary: `~/.local/bin/clipx`
- Man pages: `~/.local/share/man/man1/`
- Completions: `~/.zsh/completions/`

#### Shell Completions

**Zsh** (default shell on macOS):
Add to `~/.zshrc`:
```bash
fpath=(~/.zsh/completions $fpath)
autoload -Uz compinit && compinit
```

---

### Windows

#### Prerequisites
No additional dependencies required.

#### Installation

**Chocolatey** (coming soon):
```powershell
choco install clipx
```

**winget** (coming soon):
```powershell
winget install clipx
```

**Manual installation**:
1. Download `clipx.exe` from releases
2. Run `.\scripts\install.ps1` as Administrator

#### Installation Locations
- Binary: `%LOCALAPPDATA%\Programs\ClipX\clipx.exe`
- Added to PATH automatically

#### PowerShell Completion

Add to your PowerShell profile:
```powershell
. path\to\completions\powershell\clipx-completion.ps1
```

To find your profile location:
```powershell
$PROFILE
```

---

## Verification

After installation, verify:

```bash
# Check version
clipx --version

# Test copy
echo "Hello" | clipx copy

# Test paste
clipx paste

# View help
clipx --help

# Read man page (Linux/macOS)
man clipx
```

---

## Uninstallation

### Linux/macOS

**Using uninstall script** (recommended):
```bash
./scripts/uninstall.sh
```

**Keep your data**:
```bash
./scripts/uninstall.sh --keep-data
```

**For system installation**:
```bash
sudo ./scripts/uninstall.sh
```

**Manual uninstallation**:

**User installation**:
```bash
rm ~/.local/bin/clipx
rm ~/.local/share/man/man1/clipx*.1
rm ~/.local/share/bash-completion/completions/clipx
rm ~/.zsh/completions/_clipx
rm ~/.config/fish/completions/clipx.fish
```

**System installation**:
```bash
sudo rm /usr/local/bin/clipx
sudo rm /usr/local/share/man/man1/clipx*.1
sudo rm /etc/bash_completion.d/clipx
sudo rm /usr/local/share/zsh/site-functions/_clipx
```

**Data and configuration** (optional):
```bash
rm -rf ~/.local/share/clipx
rm -rf ~/.config/clipx
```

### Windows

**Using uninstall script** (recommended):
```powershell
.\scripts\uninstall.ps1
```

**Keep your data**:
```powershell
.\scripts\uninstall.ps1 -KeepData
```

See [WINDOWS_INSTALLATION.md](WINDOWS_INSTALLATION.md) for manual uninstallation steps.

---

## Troubleshooting

### Binary not in PATH

**Linux/macOS**:
Add to `~/.bashrc` or `~/.zshrc`:
```bash
export PATH="$HOME/.local/bin:$PATH"
```

**Windows**:
The installer should add to PATH automatically. If not, add manually:
1. Search for "Environment Variables"
2. Edit PATH
3. Add `%LOCALAPPDATA%\Programs\ClipX`

### Completions not working

**Bash**:
```bash
source ~/.bashrc
# Or
source ~/.local/share/bash-completion/completions/clipx
```

**Zsh**:
Ensure `fpath` is set before `compinit` in `~/.zshrc`

**Fish**:
```bash
exec fish
```

### Man pages not found

**Linux/macOS**:
Update man database:
```bash
mandb  # System-wide
# Or add to MANPATH in ~/.bashrc:
export MANPATH="$HOME/.local/share/man:$MANPATH"
```

---

## Building from Source

If you want to build from source instead of using the install script:

```bash
# Clone repository
git clone https://github.com/rajware/clipx.git
cd clipx

# Build
dotnet build -c Release

# Publish for your platform
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -o ./publish/linux-x64

# Run install script
./scripts/install.sh
```

---

## Next Steps

After installation:
1. Read the documentation: `man clipx`
2. Try the examples: `clipx --help`
3. Set up shell aliases (optional)
4. Configure history size (see `~/.config/clipx/config.json`)
