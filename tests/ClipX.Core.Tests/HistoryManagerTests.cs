using ClipX.Core.Interfaces;
using ClipX.Core.Models;
using ClipX.Core.Services;
using Moq;
using Xunit;

namespace ClipX.Core.Tests;

public class HistoryManagerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly Mock<IFileSystemProvider> _mockFileSystem;
    private readonly StorageManager _storage;
    private readonly ConfigurationManager _config;

    public HistoryManagerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"clipx-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _mockFileSystem = new Mock<IFileSystemProvider>();
        _mockFileSystem.Setup(f => f.GetDataDirectory()).Returns(_testDirectory);
        _mockFileSystem.Setup(f => f.GetConfigDirectory()).Returns(_testDirectory);
        _mockFileSystem.Setup(f => f.EnsureDirectoryExists(It.IsAny<string>()))
            .Callback<string>(dir => Directory.CreateDirectory(dir));

        _storage = new StorageManager(_mockFileSystem.Object);
        _config = new ConfigurationManager(_mockFileSystem.Object);
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
    public async Task AddEntryAsync_AddsEntryToHistory()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        var entry = ClipboardEntry.Create("Test content", "test-source");

        // Act
        await historyManager.AddEntryAsync(entry);
        var history = await historyManager.GetHistoryAsync();

        // Assert
        Assert.Single(history);
        Assert.Equal("Test content", history[0].Content);
    }

    [Fact]
    public async Task AddEntryAsync_WithDeduplication_RemovesDuplicates()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        var entry1 = ClipboardEntry.Create("Same content", "test-source");
        var entry2 = ClipboardEntry.Create("Same content", "test-source");

        // Act
        await historyManager.AddEntryAsync(entry1);
        await Task.Delay(100); // Ensure different timestamps
        await historyManager.AddEntryAsync(entry2);
        var history = await historyManager.GetHistoryAsync();

        // Assert
        Assert.Single(history); // Should only have one entry due to deduplication
    }

    [Fact]
    public async Task AddEntryAsync_ExceedingMaxSize_RemovesOldestEntry()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        
        // Add 26 entries (max is 25)
        for (int i = 1; i <= 26; i++)
        {
            var entry = ClipboardEntry.Create($"Entry {i}", "test-source");
            await historyManager.AddEntryAsync(entry);
        }

        // Act
        var history = await historyManager.GetHistoryAsync(limit: 0); // Get all

        // Assert
        Assert.Equal(25, history.Count); // Should be capped at 25
        Assert.Equal("Entry 26", history[0].Content); // Most recent
        Assert.Equal("Entry 2", history[24].Content); // Entry 1 should be removed
    }

    [Fact]
    public async Task GetHistoryAsync_WithLimit_ReturnsCorrectCount()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        for (int i = 1; i <= 10; i++)
        {
            await historyManager.AddEntryAsync(ClipboardEntry.Create($"Entry {i}", "test"));
        }

        // Act
        var history = await historyManager.GetHistoryAsync(limit: 5);

        // Assert
        Assert.Equal(5, history.Count);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEntriesInReverseChronologicalOrder()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("First", "test"));
        await Task.Delay(10);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Second", "test"));
        await Task.Delay(10);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Third", "test"));

        // Act
        var history = await historyManager.GetHistoryAsync();

        // Assert
        Assert.Equal("Third", history[0].Content);
        Assert.Equal("Second", history[1].Content);
        Assert.Equal("First", history[2].Content);
    }

    [Fact]
    public async Task GetEntryByIdAsync_WithValidId_ReturnsEntry()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        var entry = ClipboardEntry.Create("Test content", "test");
        await historyManager.AddEntryAsync(entry);

        // Act
        var retrieved = await historyManager.GetEntryByIdAsync(entry.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(entry.Content, retrieved.Content);
    }

    [Fact]
    public async Task GetEntryByIdAsync_WithShortId_ReturnsEntry()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        var entry = ClipboardEntry.Create("Test content", "test");
        await historyManager.AddEntryAsync(entry);
        var shortId = entry.Id.Substring(0, 8);

        // Act
        var retrieved = await historyManager.GetEntryByIdAsync(shortId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(entry.Content, retrieved.Content);
    }

    [Fact]
    public async Task GetEntryByPositionAsync_ReturnsCorrectEntry()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("First", "test"));
        await Task.Delay(10);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Second", "test"));
        await Task.Delay(10);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Third", "test"));

        // Act
        var entry1 = await historyManager.GetEntryByPositionAsync(1);
        var entry2 = await historyManager.GetEntryByPositionAsync(2);
        var entry3 = await historyManager.GetEntryByPositionAsync(3);

        // Assert
        Assert.Equal("Third", entry1?.Content);
        Assert.Equal("Second", entry2?.Content);
        Assert.Equal("First", entry3?.Content);
    }

    [Fact]
    public async Task ClearHistoryAsync_RemovesAllEntries()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Entry 1", "test"));
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Entry 2", "test"));

        // Act
        await historyManager.ClearHistoryAsync();
        var count = await historyManager.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task ClearHistoryBeforeAsync_RemovesOldEntries()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        var oldEntry = ClipboardEntry.Create("Old", "test");
        oldEntry.Timestamp = DateTime.UtcNow.AddDays(-10);
        
        var recentEntry = ClipboardEntry.Create("Recent", "test");
        recentEntry.Timestamp = DateTime.UtcNow;

        await historyManager.AddEntryAsync(oldEntry);
        await historyManager.AddEntryAsync(recentEntry);

        // Act
        await historyManager.ClearHistoryBeforeAsync(DateTime.UtcNow.AddDays(-5));
        var history = await historyManager.GetHistoryAsync();

        // Assert
        Assert.Single(history);
        Assert.Equal("Recent", history[0].Content);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var historyManager = new HistoryManager(_storage, _config);
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Entry 1", "test"));
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Entry 2", "test"));
        await historyManager.AddEntryAsync(ClipboardEntry.Create("Entry 3", "test"));

        // Act
        var count = await historyManager.GetCountAsync();

        // Assert
        Assert.Equal(3, count);
    }
}
