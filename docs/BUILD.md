# ClipX Build and Test Guide

This document provides step-by-step instructions for building and testing ClipX.

## Prerequisites Check

Before building, ensure you have:

1. **.NET 8.0 SDK** installed:
   ```bash
   dotnet --version
   # Should output 8.0.x or higher
   ```

2. **Linux only**: Clipboard tool installed:
   ```bash
   # Check for xclip (X11)
   which xclip
   
   # OR check for wl-clipboard (Wayland)
   which wl-copy
   ```

If prerequisites are missing, see [SETUP.md](SETUP.md) for installation instructions.

---

## Building the Project

### 1. Restore Dependencies
```bash
cd /home/rajch/projects/rajware/clipx
dotnet restore
```

### 2. Build All Projects
```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3. Build Release Version
```bash
dotnet build -c Release
```

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Project
```bash
# Core library tests
dotnet test tests/ClipX.Core.Tests/ClipX.Core.Tests.csproj

# Platform tests
dotnet test tests/ClipX.Platform.Tests/ClipX.Platform.Tests.csproj
```

---

## Publishing Self-Contained Executables

### For Current Platform (Linux x64)
```bash
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -o ./publish/linux-x64
```

The executable will be at: `./publish/linux-x64/clipx`

### For All Target Platforms

**Windows (amd64)**:
```powershell
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj `
  -c Release `
  -r win-x64 `
  --self-contained `
  -p:PublishSingleFile=true `
  -o ./publish/win-x64
```

**macOS (ARM64)**:
```bash
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -o ./publish/osx-arm64
```

**Linux (ARM64)**:
```bash
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r linux-arm64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -o ./publish/linux-arm64
```

---

## Manual Testing

### Test Copy Operation

1. **Copy simple text**:
   ```bash
   echo "Hello, ClipX!" | ./publish/linux-x64/clipx copy
   ```

2. **Verify clipboard** (use system clipboard viewer or):
   ```bash
   ./publish/linux-x64/clipx paste
   # Should output: Hello, ClipX!
   ```

3. **Copy file contents**:
   ```bash
   cat README.md | ./publish/linux-x64/clipx copy
   ```

### Test Paste Operation

1. **Copy something to clipboard** (using system tools or clipx copy)

2. **Paste to stdout**:
   ```bash
   ./publish/linux-x64/clipx paste
   ```

3. **Paste to file**:
   ```bash
   ./publish/linux-x64/clipx paste > test-output.txt
   cat test-output.txt
   ```

### Test Error Handling

1. **Empty stdin for copy**:
   ```bash
   echo -n "" | ./publish/linux-x64/clipx copy
   # Should show error message
   echo $?
   # Should output: 1 (failure exit code)
   ```

2. **Empty clipboard for paste**:
   ```bash
   # Clear clipboard first (platform-specific)
   # Then try to paste
   ./publish/linux-x64/clipx paste
   # Should show error message
   ```

---

## Troubleshooting Build Issues

### Issue: "dotnet: command not found"
**Solution**: Install .NET 8.0 SDK (see SETUP.md)

### Issue: Build fails with package restore errors
**Solution**: 
```bash
dotnet nuget locals all --clear
dotnet restore --force
dotnet build
```

### Issue: Tests fail on Linux with "No clipboard tool available"
**Solution**: Install xclip or wl-clipboard:
```bash
sudo apt install xclip
```

### Issue: TextCopy package not found
**Solution**: Ensure you have internet connectivity and run:
```bash
dotnet restore --force
```

---

## CI/CD Considerations

For automated builds across platforms, use GitHub Actions or similar with matrix builds:

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, macos-latest, windows-latest]
    include:
      - os: ubuntu-latest
        runtime: linux-x64
      - os: macos-latest
        runtime: osx-arm64
      - os: windows-latest
        runtime: win-x64
```

See the implementation plan for full CI/CD setup details.
