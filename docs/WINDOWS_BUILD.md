# Publishing ClipX on Windows

## PowerShell Command (Single Line)

```powershell
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish/win-x64
```

## Alternative: Using Backtick for Line Continuation

```powershell
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj `
  -c Release `
  -r win-x64 `
  --self-contained `
  -p:PublishSingleFile=true `
  -o ./publish/win-x64
```

**Note**: PowerShell uses backtick (`` ` ``) for line continuation, not backslash (`\`).

## CMD Command (Single Line)

```cmd
dotnet publish src\ClipX.CLI\ClipX.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o .\publish\win-x64
```

## Alternative: Using Caret for Line Continuation

```cmd
dotnet publish src\ClipX.CLI\ClipX.CLI.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained ^
  -p:PublishSingleFile=true ^
  -o .\publish\win-x64
```

**Note**: CMD uses caret (`^`) for line continuation.

## Git Bash on Windows (Bash Syntax)

```bash
dotnet publish src/ClipX.CLI/ClipX.CLI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -o ./publish/win-x64
```

**Note**: Git Bash uses backslash (`\`) for line continuation like Unix shells.

## Output

The executable will be created at: `.\publish\win-x64\clipx.exe`

## Testing

```powershell
# Copy
"Hello from Windows" | .\publish\win-x64\clipx.exe copy

# Paste
.\publish\win-x64\clipx.exe paste
```

---

## Common Issues

### Issue: "Missing expression after unary operator '--'"
**Cause**: Using bash-style backslash (`\`) line continuation in PowerShell  
**Solution**: Use backtick (`` ` ``) instead, or write the command on a single line

### Issue: Path separator errors
**Cause**: Using forward slashes (`/`) in CMD  
**Solution**: Use backslashes (`\`) for CMD, or use PowerShell/Git Bash which accept both
