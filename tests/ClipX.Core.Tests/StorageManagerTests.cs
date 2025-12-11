using ClipX.Core.Interfaces;
using ClipX.Core.Models;
using ClipX.Core.Services;
using Moq;
using Xunit;

namespace ClipX.Core.Tests;

public class StorageManagerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly Mock<IFileSystemProvider> _mockFileSystem;
    private readonly StorageManager _storage;

    public StorageManagerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"clipx-storage-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _mockFileSystem = new Mock<IFileSystemProvider>();
        _mockFileSystem.Setup(f => f.GetDataDirectory()).Returns(_testDirectory);
        _mockFileSystem.Setup(f => f.EnsureDirectoryExists(It.IsAny<string>()))
            .Callback<string>(dir => Directory.CreateDirectory(dir));

        _storage = new StorageManager(_mockFileSystem.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task SaveEntryAsync_CreatesFile()
    {
        // Arrange
        var entry = ClipboardEntry.Create("Test content", "test-source");

        // Act
        await _storage.SaveEntryAsync(entry);

        // Assert
        var historyFile = Path.Combine(_testDirectory, "clipboard-history.jsonl");
        Assert.True(File.Exists(historyFile));
    }

    [Fact]
    public async Task SaveEntryAsync_AppendsToFile()
    {
        // Arrange
        var entry1 = ClipboardEntry.Create("First", "test");
        var entry2 = ClipboardEntry.Create("Second", "test");

        // Act
        await _storage.SaveEntryAsync(entry1);
        await _storage.SaveEntryAsync(entry2);

        // Assert
        var entries = await _storage.LoadEntriesAsync();
        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public async Task LoadEntriesAsync_WithNoFile_ReturnsEmptyList()
    {
        // Act
        var entries = await _storage.LoadEntriesAsync();

        // Assert
        Assert.Empty(entries);
    }

    [Fact]
    public async Task LoadEntriesAsync_ReturnsAllEntries()
    {
        // Arrange
        var entry1 = ClipboardEntry.Create("First", "test");
        var entry2 = ClipboardEntry.Create("Second", "test");
        await _storage.SaveEntryAsync(entry1);
        await _storage.SaveEntryAsync(entry2);

        // Act
        var entries = await _storage.LoadEntriesAsync();

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Equal("First", entries[0].Content);
        Assert.Equal("Second", entries[1].Content);
    }

    [Fact]
    public async Task LoadEntriesAsync_SkipsCorruptedLines()
    {
        // Arrange
        var historyFile = Path.Combine(_testDirectory, "clipboard-history.jsonl");
        await File.WriteAllLinesAsync(historyFile, new[]
        {
            "{\"id\":\"test\",\"content\":\"Valid\",\"timestamp\":\"2024-01-01T00:00:00Z\",\"source\":\"test\",\"sizeBytes\":5,\"contentHash\":\"abc\"}",
            "corrupted line",
            "{\"id\":\"test2\",\"content\":\"Also Valid\",\"timestamp\":\"2024-01-01T00:00:00Z\",\"source\":\"test\",\"sizeBytes\":10,\"contentHash\":\"def\"}"
        });

        // Act
        var entries = await _storage.LoadEntriesAsync();

        // Assert
        Assert.Equal(2, entries.Count); // Should skip the corrupted line
    }

    [Fact]
    public async Task SaveAllEntriesAsync_ReplacesFile()
    {
        // Arrange
        var entry1 = ClipboardEntry.Create("First", "test");
        await _storage.SaveEntryAsync(entry1);

        var newEntries = new List<ClipboardEntry>
        {
            ClipboardEntry.Create("Replaced 1", "test"),
            ClipboardEntry.Create("Replaced 2", "test")
        };

        // Act
        await _storage.SaveAllEntriesAsync(newEntries);
        var loaded = await _storage.LoadEntriesAsync();

        // Assert
        Assert.Equal(2, loaded.Count);
        Assert.Equal("Replaced 1", loaded[0].Content);
        Assert.Equal("Replaced 2", loaded[1].Content);
    }

    [Fact]
    public async Task ClearStorageAsync_DeletesFile()
    {
        // Arrange
        var entry = ClipboardEntry.Create("Test", "test");
        await _storage.SaveEntryAsync(entry);
        var historyFile = Path.Combine(_testDirectory, "clipboard-history.jsonl");
        Assert.True(File.Exists(historyFile));

        // Act
        await _storage.ClearStorageAsync();

        // Assert
        Assert.False(File.Exists(historyFile));
    }

    [Fact]
    public async Task ClearStorageAsync_WithNoFile_DoesNotThrow()
    {
        // Act & Assert
        await _storage.ClearStorageAsync(); // Should not throw
    }
}
