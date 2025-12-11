using System.Runtime.InteropServices;
using ClipX.Core.Interfaces;

namespace ClipX.Platform;

/// <summary>
/// Factory for creating platform-specific provider implementations.
/// </summary>
public static class PlatformFactory
{
    /// <summary>
    /// Creates the appropriate clipboard provider for the current platform.
    /// </summary>
    /// <returns>Platform-specific clipboard provider.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    public static IClipboardProvider CreateClipboardProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new Windows.WindowsClipboardProvider();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOS.MacOSClipboardProvider();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new Linux.LinuxClipboardProvider();
        }
        else
        {
            throw new PlatformNotSupportedException($"Platform {RuntimeInformation.OSDescription} is not supported");
        }
    }

    /// <summary>
    /// Creates the file system provider.
    /// </summary>
    /// <returns>File system provider instance.</returns>
    public static IFileSystemProvider CreateFileSystemProvider()
    {
        return new PlatformFileSystemProvider();
    }

    /// <summary>
    /// Gets a platform identifier string for the current system.
    /// </summary>
    /// <returns>Platform identifier (e.g., "windows-x64", "linux-arm64").</returns>
    public static string GetPlatformIdentifier()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" :
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" :
                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" : "unknown";

        var arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

        return $"{os}-{arch}";
    }
}
