using System.Runtime.Versioning;
using ClipX.Core.Interfaces;

namespace ClipX.Platform.Windows;

/// <summary>
/// Windows-specific clipboard implementation using TextCopy library.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsClipboardProvider : IClipboardProvider
{
    public async Task<string> GetTextAsync()
    {
        try
        {
            // Use TextCopy library for cross-platform clipboard access
            // This will be added as a NuGet package
            return await Task.Run(() => TextCopy.ClipboardService.GetText() ?? string.Empty);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get clipboard text on Windows: {ex.Message}", ex);
        }
    }

    public async Task SetTextAsync(string text)
    {
        try
        {
            // Use TextCopy library for cross-platform clipboard access
            await Task.Run(() => TextCopy.ClipboardService.SetText(text));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set clipboard text on Windows: {ex.Message}", ex);
        }
    }
}
