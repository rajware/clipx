using System.Text.Json;
using ClipX.Core.Interfaces;
using ClipX.Core.Models;

namespace ClipX.Core.Services;

/// <summary>
/// Manages persistent storage of clipboard history using JSON Lines format.
/// </summary>
public class StorageManager
{
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly string _historyFilePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false
    };

    public StorageManager(IFileSystemProvider fileSystemProvider)
    {
        _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
        
        var dataDir = _fileSystemProvider.GetDataDirectory();
        _fileSystemProvider.EnsureDirectoryExists(dataDir);
        _historyFilePath = Path.Combine(dataDir, "clipboard-history.jsonl");
    }

    /// <summary>
    /// Appends a clipboard entry to the history file.
    /// </summary>
    public async Task SaveEntryAsync(ClipboardEntry entry)
    {
        try
        {
            var json = JsonSerializer.Serialize(entry, _jsonOptions);
            await File.AppendAllLinesAsync(_historyFilePath, new[] { json });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save clipboard entry: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads all clipboard entries from the history file.
    /// </summary>
    /// <returns>List of clipboard entries, ordered from oldest to newest.</returns>
    public async Task<List<ClipboardEntry>> LoadEntriesAsync()
    {
        if (!File.Exists(_historyFilePath))
        {
            return new List<ClipboardEntry>();
        }

        var entries = new List<ClipboardEntry>();

        try
        {
            var lines = await File.ReadAllLinesAsync(_historyFilePath);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var entry = JsonSerializer.Deserialize<ClipboardEntry>(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
                catch
                {
                    // Skip corrupted lines
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load clipboard history: {ex.Message}", ex);
        }

        return entries;
    }

    /// <summary>
    /// Saves all entries to the history file, replacing existing content.
    /// </summary>
    public async Task SaveAllEntriesAsync(IEnumerable<ClipboardEntry> entries)
    {
        try
        {
            var lines = entries.Select(e => JsonSerializer.Serialize(e, _jsonOptions));
            await File.WriteAllLinesAsync(_historyFilePath, lines);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save clipboard history: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Clears all clipboard history by deleting the history file.
    /// </summary>
    public async Task ClearStorageAsync()
    {
        try
        {
            if (File.Exists(_historyFilePath))
            {
                await Task.Run(() => File.Delete(_historyFilePath));
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to clear clipboard history: {ex.Message}", ex);
        }
    }
}
