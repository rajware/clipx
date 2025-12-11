using System.Diagnostics;
using System.Runtime.Versioning;
using ClipX.Core.Interfaces;

namespace ClipX.Platform.Linux;

/// <summary>
/// Linux-specific clipboard implementation using xclip or wl-clipboard.
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxClipboardProvider : IClipboardProvider
{
    private readonly ClipboardTool _tool;

    public LinuxClipboardProvider()
    {
        _tool = DetectClipboardTool();
    }

    private enum ClipboardTool
    {
        XClip,
        WlClipboard,
        None
    }

    private static ClipboardTool DetectClipboardTool()
    {
        // Check for Wayland first (wl-copy/wl-paste)
        if (IsCommandAvailable("wl-copy"))
        {
            return ClipboardTool.WlClipboard;
        }

        // Fall back to X11 (xclip)
        if (IsCommandAvailable("xclip"))
        {
            return ClipboardTool.XClip;
        }

        return ClipboardTool.None;
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetTextAsync()
    {
        return _tool switch
        {
            ClipboardTool.XClip => await GetTextXClipAsync(),
            ClipboardTool.WlClipboard => await GetTextWlClipboardAsync(),
            _ => throw new InvalidOperationException(
                "No clipboard tool available. Please install xclip (for X11) or wl-clipboard (for Wayland)")
        };
    }

    public async Task SetTextAsync(string text)
    {
        switch (_tool)
        {
            case ClipboardTool.XClip:
                await SetTextXClipAsync(text);
                break;
            case ClipboardTool.WlClipboard:
                await SetTextWlClipboardAsync(text);
                break;
            default:
                throw new InvalidOperationException(
                    "No clipboard tool available. Please install xclip (for X11) or wl-clipboard (for Wayland)");
        }
    }

    private async Task<string> GetTextXClipAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard -o",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start xclip process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // xclip returns exit code 1 when clipboard is empty
            if (process.ExitCode == 1 && string.IsNullOrEmpty(output))
            {
                return string.Empty;
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"xclip exited with code {process.ExitCode}: {error}");
            }

            return output;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get clipboard text using xclip: {ex.Message}", ex);
        }
    }

    private async Task SetTextXClipAsync(string text)
    {
        try
        {
            // xclip forks to background by default, so we need to handle this properly
            // Use POSIX-compliant /bin/sh (not bash) for maximum compatibility
            // This works on Alpine Linux, BusyBox, and all POSIX-compliant systems
            // Using printf (POSIX) instead of echo for better portability
            var escapedText = text.Replace("'", "'\\''");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"printf '%s' '{escapedText}' | xclip -selection clipboard 2>/dev/null &\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start shell process for xclip");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Shell exited with code {process.ExitCode}");
            }
            
            // Give xclip a moment to actually copy the data
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set clipboard text using xclip: {ex.Message}", ex);
        }
    }

    private async Task<string> GetTextWlClipboardAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "wl-paste",
                Arguments = "--no-newline",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start wl-paste process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // wl-paste returns non-zero when clipboard is empty
            if (process.ExitCode != 0 && string.IsNullOrEmpty(output))
            {
                return string.Empty;
            }

            return output;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get clipboard text using wl-paste: {ex.Message}", ex);
        }
    }

    private async Task SetTextWlClipboardAsync(string text)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "wl-copy",
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start wl-copy process");
            }

            await process.StandardInput.WriteAsync(text);
            process.StandardInput.Close();

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"wl-copy exited with code {process.ExitCode}: {error}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set clipboard text using wl-copy: {ex.Message}", ex);
        }
    }
}
