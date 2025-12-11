# ClipX Development Setup

## Prerequisites

### .NET 8.0 SDK

ClipX requires .NET 8.0 SDK to build and run.

#### Installation

**Linux (Ubuntu/Debian)**:
```bash
sudo apt update
sudo apt install dotnet-sdk-8.0
```

**Linux (Fedora)**:
```bash
sudo dnf install dotnet-sdk-8.0
```

**macOS**:
```bash
brew install dotnet@8
```

**Windows**:
Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

### Platform-Specific Dependencies

#### Linux
ClipX requires either `xclip` (for X11) or `wl-clipboard` (for Wayland):

**X11 (most desktop environments)**:
```bash
sudo apt install xclip        # Ubuntu/Debian
sudo dnf install xclip        # Fedora
```

**Wayland (GNOME on Wayland, Sway, etc.)**:
```bash
sudo apt install wl-clipboard # Ubuntu/Debian
sudo dnf install wl-clipboard # Fedora
```

#### macOS
No additional dependencies required. ClipX uses the built-in `pbcopy` and `pbpaste` commands.

#### Windows
No additional dependencies required. ClipX uses the Windows clipboard API via the TextCopy library.

---

## Building ClipX

### Build All Projects
```bash
dotnet build
```

### Build Release Version
```bash
dotnet build -c Release
```

### Build for Specific Platform
```bash
# Windows (amd64)
dotnet build -c Release -r win-x64

# macOS (ARM64)
dotnet build -c Release -r osx-arm64

# Linux (amd64)
dotnet build -c Release -r linux-x64

# Linux (ARM64)
dotnet build -c Release -r linux-arm64
```

---

## Running ClipX

### Run from Source
```bash
dotnet run --project src/ClipX.CLI/ClipX.CLI.csproj -- copy
dotnet run --project src/ClipX.CLI/ClipX.CLI.csproj -- paste
```

### Run Published Binary
```bash
# Publish self-contained executable
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj -c Release -r linux-x64 --self-contained -o ./publish

# Run the executable
./publish/clipx copy
./publish/clipx paste
```

---

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Project
```bash
dotnet test tests/ClipX.Core.Tests/ClipX.Core.Tests.csproj
dotnet test tests/ClipX.Platform.Tests/ClipX.Platform.Tests.csproj
```

---

## Usage Examples

### Copy text to clipboard
```bash
echo "Hello, World!" | ./clipx copy
cat file.txt | ./clipx copy
```

### Paste text from clipboard
```bash
./clipx paste
./clipx paste > output.txt
```

---

## Project Structure

```
clipx/
├── src/
│   ├── ClipX.Core/          # Platform-agnostic core library
│   ├── ClipX.Platform/      # Platform-specific implementations
│   ├── ClipX.CLI/           # Command-line interface
│   └── ClipX.Server/        # Server (future)
├── tests/
│   ├── ClipX.Core.Tests/
│   ├── ClipX.Platform.Tests/
│   └── ClipX.CLI.Tests/
├── docs/                    # Documentation
└── build/                   # Build outputs
```

---

## Troubleshooting

### Linux: "No clipboard tool available"
Install either `xclip` or `wl-clipboard` depending on your display server:
- X11: `sudo apt install xclip`
- Wayland: `sudo apt install wl-clipboard`

### macOS: "pbcopy: command not found"
This should not happen on macOS. Ensure you're running on a genuine macOS system.

### Windows: Clipboard access errors
Ensure no other application is blocking clipboard access. Try running as administrator if issues persist.

---

## Next Steps

- **Stage 2**: Implement clipboard history management
- **Stage 3**: Implement sync functionality with ClipX.Server
