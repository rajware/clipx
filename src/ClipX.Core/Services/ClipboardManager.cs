using ClipX.Core.Interfaces;
using ClipX.Core.Models;

namespace ClipX.Core.Services;

/// <summary>
/// Manages high-level clipboard operations.
/// </summary>
public class ClipboardManager
{
    private readonly IClipboardProvider _clipboardProvider;
    private readonly HistoryManager _historyManager;
    private readonly string _sourceIdentifier;

    public ClipboardManager(
        IClipboardProvider clipboardProvider,
        HistoryManager historyManager,
        string sourceIdentifier)
    {
        _clipboardProvider = clipboardProvider ?? throw new ArgumentNullException(nameof(clipboardProvider));
        _historyManager = historyManager ?? throw new ArgumentNullException(nameof(historyManager));
        _sourceIdentifier = sourceIdentifier ?? throw new ArgumentNullException(nameof(sourceIdentifier));
    }

    /// <summary>
    /// Reads text from stdin and copies it to the clipboard.
    /// </summary>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task<bool> CopyFromStdinAsync()
    {
        try
        {
            // Read all text from stdin
            using var reader = Console.In;
            var content = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(content))
            {
                Console.Error.WriteLine("Error: No input provided");
                return false;
            }

            // Copy to clipboard
            await _clipboardProvider.SetTextAsync(content);

            // Add to history
            var entry = ClipboardEntry.Create(content, _sourceIdentifier);
            await _historyManager.AddEntryAsync(entry);

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error copying to clipboard: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reads text from the clipboard and writes it to stdout.
    /// </summary>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task<bool> PasteToStdoutAsync()
    {
        try
        {
            // Get clipboard content
            var content = await _clipboardProvider.GetTextAsync();

            if (string.IsNullOrEmpty(content))
            {
                Console.Error.WriteLine("Error: Clipboard is empty");
                return false;
            }

            // Write to stdout
            await Console.Out.WriteAsync(content);

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading from clipboard: {ex.Message}");
            return false;
        }
    }
}
