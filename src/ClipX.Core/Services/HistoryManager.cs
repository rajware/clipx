using ClipX.Core.Models;

namespace ClipX.Core.Services;

/// <summary>
/// Manages clipboard history with persistence, round-robin, and deduplication.
/// </summary>
public class HistoryManager
{
    private readonly StorageManager _storageManager;
    private readonly ConfigurationManager _configurationManager;
    private List<ClipboardEntry> _entries;
    private bool _isLoaded;

    public HistoryManager(StorageManager storageManager, ConfigurationManager configurationManager)
    {
        _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        _entries = new List<ClipboardEntry>();
        _isLoaded = false;
    }

    /// <summary>
    /// Ensures history is loaded from storage.
    /// </summary>
    private async Task EnsureLoadedAsync()
    {
        if (!_isLoaded)
        {
            _entries = await _storageManager.LoadEntriesAsync();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Adds a clipboard entry to history.
    /// Implements deduplication and round-robin based on configuration.
    /// </summary>
    public async Task AddEntryAsync(ClipboardEntry entry)
    {
        await EnsureLoadedAsync();
        var config = await _configurationManager.LoadConfigAsync();

        // Check for deduplication
        if (config.EnableDeduplication)
        {
            var existing = _entries.FirstOrDefault(e => e.ContentHash == entry.ContentHash);
            if (existing != null)
            {
                // Remove old entry and add new one (updates timestamp)
                _entries.Remove(existing);
            }
        }

        // Add new entry
        _entries.Add(entry);

        // Implement round-robin: remove oldest entries if exceeding max size
        while (_entries.Count > config.MaxHistorySize)
        {
            _entries.RemoveAt(0); // Remove oldest
        }

        // Persist to storage
        await _storageManager.SaveAllEntriesAsync(_entries);
    }

    /// <summary>
    /// Gets clipboard history entries, most recent first.
    /// </summary>
    /// <param name="limit">Maximum number of entries to return. 0 = all entries.</param>
    public async Task<List<ClipboardEntry>> GetHistoryAsync(int limit = 10)
    {
        await EnsureLoadedAsync();

        var result = _entries
            .OrderByDescending(e => e.Timestamp)
            .AsEnumerable();

        if (limit > 0)
        {
            result = result.Take(limit);
        }

        return result.ToList();
    }

    /// <summary>
    /// Gets a specific clipboard entry by ID.
    /// </summary>
    public async Task<ClipboardEntry?> GetEntryByIdAsync(string id)
    {
        await EnsureLoadedAsync();

        // Support both full ID and short ID (first 8 characters)
        return _entries.FirstOrDefault(e => 
            e.Id == id || 
            e.Id.StartsWith(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a clipboard entry by position (1 = most recent).
    /// </summary>
    public async Task<ClipboardEntry?> GetEntryByPositionAsync(int position)
    {
        await EnsureLoadedAsync();

        if (position < 1 || position > _entries.Count)
        {
            return null;
        }

        var sorted = _entries.OrderByDescending(e => e.Timestamp).ToList();
        return sorted[position - 1];
    }

    /// <summary>
    /// Clears all clipboard history.
    /// </summary>
    public async Task ClearHistoryAsync()
    {
        _entries.Clear();
        _isLoaded = true;
        await _storageManager.ClearStorageAsync();
    }

    /// <summary>
    /// Clears clipboard history entries before a specific date.
    /// </summary>
    public async Task ClearHistoryBeforeAsync(DateTime before)
    {
        await EnsureLoadedAsync();

        _entries.RemoveAll(e => e.Timestamp < before);
        await _storageManager.SaveAllEntriesAsync(_entries);
    }

    /// <summary>
    /// Gets the total number of entries in history.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        await EnsureLoadedAsync();
        return _entries.Count;
    }
}
