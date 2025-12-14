# Windows Installation Guide for ClipX

## Quick Install

### Option 1: PowerShell Script (Recommended)

**User installation** (no admin required):
```powershell
.\scripts\install.ps1
```

**System-wide installation** (run PowerShell as Administrator):
```powershell
.\scripts\install.ps1 -System
```

### Option 2: Manual Installation

1. Download `clipx.exe` from the latest release
2. Create directory: `%LOCALAPPDATA%\Programs\ClipX`
3. Copy `clipx.exe` to that directory
4. Add to PATH (see below)

---

## Prerequisites

### .NET Runtime
ClipX is published as a self-contained executable, so **no .NET runtime is required**.

### Building from Source
If building from source, you need:
- .NET 8.0 SDK
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0

---

## Installation Details

### User Installation (Default)

**Location**: `%LOCALAPPDATA%\Programs\ClipX`  
**Full path**: `C:\Users\<YourName>\AppData\Local\Programs\ClipX`

**What gets installed**:
- `clipx.exe` - Main executable
- PowerShell completion script
- Added to user PATH

**Advantages**:
- No admin rights required
- Per-user installation
- Easy to uninstall

### System Installation

**Location**: `%ProgramFiles%\ClipX`  
**Full path**: `C:\Program Files\ClipX`

**What gets installed**:
- `clipx.exe` - Main executable
- PowerShell completion script
- Added to system PATH

**Advantages**:
- Available to all users
- Standard program location

**Requirements**:
- Administrator privileges

---

## PowerShell Completion

### Automatic Setup (via install script)
The install script automatically:
1. Copies completion script to your PowerShell profile directory
2. Adds it to your `$PROFILE`
3. Enables tab completion

### Manual Setup

1. Copy completion script:
```powershell
$profileDir = Split-Path $PROFILE -Parent
Copy-Item completions\powershell\clipx-completion.ps1 "$profileDir\clipx-completion.ps1"
```

2. Add to your PowerShell profile:
```powershell
# Create profile if it doesn't exist
if (-not (Test-Path $PROFILE)) {
    New-Item -Path $PROFILE -ItemType File -Force
}

# Add completion
Add-Content $PROFILE "`n. `"$profileDir\clipx-completion.ps1`""
```

3. Reload profile:
```powershell
. $PROFILE
```

### Finding Your Profile
```powershell
# Show profile location
$PROFILE

# Edit profile
notepad $PROFILE
```

---

## Adding to PATH Manually

If the install script didn't add ClipX to PATH:

### User PATH
```powershell
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
$clipxPath = "$env:LOCALAPPDATA\Programs\ClipX"
[Environment]::SetEnvironmentVariable("Path", "$clipxPath;$userPath", "User")
```

### System PATH (requires Administrator)
```powershell
$systemPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
$clipxPath = "$env:ProgramFiles\ClipX"
[Environment]::SetEnvironmentVariable("Path", "$clipxPath;$systemPath", "Machine")
```

### Verify PATH
```powershell
# Restart PowerShell, then:
Get-Command clipx
clipx --version
```

---

## Verification

After installation:

```powershell
# Check version
clipx --version

# Test copy
"Hello from Windows" | clipx copy

# Test paste
clipx paste

# View help
clipx --help

# Test completion (type and press TAB)
clipx <TAB>
```

---

## Data Locations

### Clipboard History
```
%LOCALAPPDATA%\ClipX\clipboard-history.jsonl
C:\Users\<YourName>\AppData\Local\ClipX\clipboard-history.jsonl
```

### Configuration
```
%LOCALAPPDATA%\ClipX\config.json
C:\Users\<YourName>\AppData\Local\ClipX\config.json
```

---

## Uninstallation

### Using Uninstall Script
```powershell
.\scripts\uninstall.ps1
```

**Keep your data**:
```powershell
.\scripts\uninstall.ps1 -KeepData
```

### Manual Uninstallation

1. **Remove binary**:
```powershell
# User installation
Remove-Item "$env:LOCALAPPDATA\Programs\ClipX" -Recurse

# System installation (as Administrator)
Remove-Item "$env:ProgramFiles\ClipX" -Recurse
```

2. **Remove from PATH**:
```powershell
# User PATH
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
$newPath = ($userPath -split ';' | Where-Object { $_ -notlike "*ClipX*" }) -join ';'
[Environment]::SetEnvironmentVariable("Path", $newPath, "User")
```

3. **Remove completion**:
```powershell
$profileDir = Split-Path $PROFILE -Parent
Remove-Item "$profileDir\clipx-completion.ps1"
```

4. **Remove from profile**:
Edit `$PROFILE` and remove the line:
```powershell
. "...\clipx-completion.ps1"
```

5. **Remove data** (optional):
```powershell
Remove-Item "$env:LOCALAPPDATA\ClipX" -Recurse
```

---

## Troubleshooting

### "clipx is not recognized"

**Solution 1**: Restart PowerShell
```powershell
# Close and reopen PowerShell
```

**Solution 2**: Verify PATH
```powershell
$env:Path -split ';' | Select-String ClipX
```

**Solution 3**: Add to PATH manually (see above)

### Completion not working

**Check if loaded**:
```powershell
Get-Content $PROFILE
```

**Reload profile**:
```powershell
. $PROFILE
```

**Verify completion script exists**:
```powershell
$profileDir = Split-Path $PROFILE -Parent
Test-Path "$profileDir\clipx-completion.ps1"
```

### "Execution policy" error

```powershell
# Check current policy
Get-ExecutionPolicy

# Set policy (as Administrator)
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Permission denied

**For user installation**: No admin required  
**For system installation**: Run PowerShell as Administrator

---

## Building from Source

```powershell
# Clone repository
git clone https://github.com/rajware/clipx.git
cd clipx

# Build
dotnet build -c Release

# Publish
dotnet publish src\ClipX.CLI\ClipX.CLI.csproj `
  -c Release `
  -r win-x64 `
  --self-contained `
  -p:PublishSingleFile=true `
  -o .\publish\win-x64

# Install
.\scripts\install.ps1
```

---

## Windows Terminal Integration

### Add to Windows Terminal settings

1. Open Windows Terminal settings (Ctrl+,)
2. Add to your profile:

```json
{
  "commandline": "powershell.exe -NoExit -Command \"& {. $PROFILE}\"",
  "name": "PowerShell with ClipX"
}
```

### Suggested Aliases

Add to your `$PROFILE`:

```powershell
# ClipX aliases
Set-Alias -Name cc -Value clipx
function Copy-Clip { clipx copy }
function Paste-Clip { clipx paste }
function Show-ClipHistory { clipx history }

# Usage:
# "text" | Copy-Clip
# Paste-Clip
# Show-ClipHistory
```

---

## Next Steps

1. ✅ Install ClipX
2. ✅ Verify it works
3. ✅ Set up completions
4. 📝 Configure history size (edit `%LOCALAPPDATA%\ClipX\config.json`)
5. 🎯 Integrate into your workflow

---

## Support

- Documentation: `clipx --help`
- Issues: https://github.com/rajware/clipx/issues
- Online docs: https://github.com/rajware/clipx
