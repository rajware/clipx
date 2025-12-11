using ClipX.Core.Models;

namespace ClipX.Core.Services;

/// <summary>
/// Manages clipboard history (basic implementation for Stage 1).
/// </summary>
public class HistoryManager
{
    // For Stage 1, this is a simple placeholder
    // Stage 2 will implement full history management with persistence

    /// <summary>
    /// Adds a clipboard entry to the history.
    /// </summary>
    /// <param name="entry">The clipboard entry to add.</param>
    public Task AddEntryAsync(ClipboardEntry entry)
    {
        // Placeholder for Stage 1
        // In Stage 2, this will persist to storage
        return Task.CompletedTask;
    }
}
