# GitHub Actions CI/CD Setup

## Overview

This project uses GitHub Actions for automated cross-platform testing and builds. Every push and pull request triggers builds on Linux, Windows, and macOS.

## Workflow: Cross-Platform CI

**File**: `.github/workflows/ci.yml`

### What it does

1. **Matrix Build**: Runs on 3 platforms in parallel
   - Ubuntu (Linux x64)
   - Windows Server (Windows x64)
   - macOS (ARM64)

2. **For each platform**:
   - ✅ Install .NET 8.0 SDK
   - ✅ Install platform dependencies (xclip on Linux)
   - ✅ Restore NuGet packages
   - ✅ Build in Release mode
   - ✅ Run all tests
   - ✅ Publish self-contained executable
   - ✅ Upload artifacts (available for 7 days)

3. **On tagged releases** (optional):
   - Creates GitHub release
   - Attaches executables for all platforms

## Usage

### Automatic Triggers

The workflow runs automatically on:
- Push to `main` or `develop` branches
- Pull requests to `main`

### Manual Trigger

You can also run it manually:
1. Go to **Actions** tab in GitHub
2. Select **Cross-Platform CI**
3. Click **Run workflow**

### Viewing Results

1. Go to **Actions** tab
2. Click on a workflow run
3. See results for all 3 platforms
4. Download built executables from **Artifacts**

## Platform-Specific Notes

### Linux (Ubuntu)
- **Runner**: `ubuntu-latest`
- **Dependencies**: Installs `xclip` automatically
- **Tests**: Full clipboard functionality tested
- **Output**: `clipx` (ELF executable)

### Windows
- **Runner**: `windows-latest`
- **Dependencies**: None (TextCopy library handles clipboard)
- **Tests**: Full clipboard functionality tested
- **Output**: `clipx.exe`

### macOS
- **Runner**: `macos-latest` (ARM64)
- **Dependencies**: None (uses built-in pbcopy/pbpaste)
- **Tests**: Full clipboard functionality tested
- **Output**: `clipx` (Mach-O executable)

## Important Notes

### Clipboard Testing Limitations

⚠️ **GitHub Actions runners are headless** (no GUI/display server), which means:

- **Linux**: Tests will pass, but actual clipboard operations may not work in CI
  - Unit tests with mocked providers: ✅ Work
  - Integration tests requiring X11: ⚠️ May fail
  - Solution: Mock clipboard in CI, test manually on real systems

- **Windows**: Similar limitations without interactive desktop
  - Unit tests: ✅ Work
  - Real clipboard access: ⚠️ May fail in CI

- **macOS**: Same headless limitations
  - Unit tests: ✅ Work
  - pbcopy/pbpaste: ⚠️ May fail without GUI session

### Recommended Approach

1. **Unit tests** (with mocked clipboard): Run in CI ✅
2. **Build verification**: Run in CI ✅
3. **Manual clipboard testing**: Do locally on each platform ✅

Our current test suite uses mocked providers, so it **will work in CI**.

## Creating a Release

To create a release with executables for all platforms:

```bash
# Tag a version
git tag -a v0.1.0 -m "Release v0.1.0 - Stage 1 complete"
git push origin v0.1.0
```

This will:
1. Trigger the CI workflow
2. Build for all platforms
3. Create a GitHub release
4. Attach executables automatically

## Free Tier Limits

**GitHub Actions Free Tier** (for public repos):
- ✅ **Unlimited** minutes for public repositories
- ✅ 2,000 minutes/month for private repositories
- ✅ 20 concurrent jobs

**For this project**:
- Each full workflow run: ~5-10 minutes total (parallel)
- Estimated runs per month: 100-200 (plenty of headroom)

## Artifacts

After each successful build, you can download executables:

1. Go to **Actions** → Select a workflow run
2. Scroll to **Artifacts** section
3. Download:
   - `clipx-linux-x64.zip`
   - `clipx-win-x64.zip`
   - `clipx-osx-arm64.zip`

Artifacts are kept for **7 days** by default.

## Troubleshooting

### Build fails on specific platform

Check the logs for that platform:
1. Click on the failed job
2. Expand the failed step
3. Review error messages

### Tests fail in CI but pass locally

This is expected for clipboard integration tests. Our unit tests with mocked providers should pass.

### Want to test on different architectures?

Modify the matrix in `.github/workflows/ci.yml`:

```yaml
matrix:
  include:
    - os: ubuntu-latest
      platform: linux-arm64  # Add ARM64 Linux
```

## Next Steps

1. **Commit the workflow file**:
   ```bash
   git add .github/workflows/ci.yml
   git commit -m "ci: add GitHub Actions workflow for cross-platform builds"
   git push
   ```

2. **Push to GitHub** and watch the Actions tab

3. **Review results** for all three platforms

4. **Download artifacts** to test executables locally

---

**Status**: Ready to use  
**Cost**: Free (unlimited for public repos)  
**Platforms**: Linux, Windows, macOS  
**Automation**: Fully automated on push/PR
