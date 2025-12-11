namespace ClipX.Core.Models;

/// <summary>
/// Application configuration settings.
/// </summary>
public class ClipXConfiguration
{
    /// <summary>
    /// Maximum number of entries to keep in history.
    /// Older entries are removed when this limit is exceeded (round-robin).
    /// </summary>
    public int MaxHistorySize { get; set; } = 25;

    /// <summary>
    /// Whether to enable deduplication of clipboard entries.
    /// When enabled, duplicate content (same hash) will not be added to history.
    /// </summary>
    public bool EnableDeduplication { get; set; } = true;
}
