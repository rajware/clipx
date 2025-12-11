namespace ClipX.Core.Interfaces;

/// <summary>
/// Provides platform-specific clipboard operations.
/// </summary>
public interface IClipboardProvider
{
    /// <summary>
    /// Gets the current text content from the system clipboard.
    /// </summary>
    /// <returns>The clipboard text content, or empty string if clipboard is empty or contains non-text data.</returns>
    Task<string> GetTextAsync();

    /// <summary>
    /// Sets the text content to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy to the clipboard.</param>
    Task SetTextAsync(string text);
}
