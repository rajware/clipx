using System.Diagnostics;
using System.Runtime.Versioning;
using ClipX.Core.Interfaces;

namespace ClipX.Platform.MacOS;

/// <summary>
/// macOS-specific clipboard implementation using pbcopy/pbpaste.
/// </summary>
[SupportedOSPlatform("macos")]
public class MacOSClipboardProvider : IClipboardProvider
{
    public async Task<string> GetTextAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pbpaste",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start pbpaste process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"pbpaste exited with code {process.ExitCode}");
            }

            return output;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get clipboard text on macOS: {ex.Message}", ex);
        }
    }

    public async Task SetTextAsync(string text)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pbcopy",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start pbcopy process");
            }

            await process.StandardInput.WriteAsync(text);
            process.StandardInput.Close();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"pbcopy exited with code {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set clipboard text on macOS: {ex.Message}", ex);
        }
    }
}
