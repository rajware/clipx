using ClipX.Core.Interfaces;
using ClipX.Core.Services;
using Moq;
using Xunit;

namespace ClipX.Core.Tests;

public class ClipboardManagerTests : IDisposable
{
    private readonly string _testDirectory;

    public ClipboardManagerTests()
    {
        // Create a unique temporary directory for each test run
        _testDirectory = Path.Combine(Path.GetTempPath(), $"clipx-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // Clean up test directory after each test
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

    private HistoryManager CreateTestHistoryManager()
    {
        var mockFileSystem = new Mock<IFileSystemProvider>();
        mockFileSystem.Setup(f => f.GetDataDirectory()).Returns(_testDirectory);
        mockFileSystem.Setup(f => f.GetConfigDirectory()).Returns(_testDirectory);
        mockFileSystem.Setup(f => f.EnsureDirectoryExists(It.IsAny<string>()))
            .Callback<string>(dir => Directory.CreateDirectory(dir));
        
        var storage = new StorageManager(mockFileSystem.Object);
        var config = new ConfigurationManager(mockFileSystem.Object);
        
        return new HistoryManager(storage, config);
    }

    [Fact]
    public async Task CopyFromStdin_WithValidInput_ReturnsTrue()
    {
        // Arrange
        var mockClipboard = new Mock<IClipboardProvider>();
        var historyManager = CreateTestHistoryManager();
        var manager = new ClipboardManager(mockClipboard.Object, historyManager, "test-platform");

        var testInput = "Test clipboard content";
        var originalIn = Console.In;
        try
        {
            // Redirect stdin
            using var stringReader = new StringReader(testInput);
            Console.SetIn(stringReader);

            // Act
            var result = await manager.CopyFromStdinAsync();

            // Assert
            Assert.True(result);
            mockClipboard.Verify(c => c.SetTextAsync(testInput), Times.Once);
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public async Task PasteToStdout_WithClipboardContent_ReturnsTrue()
    {
        // Arrange
        var mockClipboard = new Mock<IClipboardProvider>();
        var testContent = "Clipboard content";
        mockClipboard.Setup(c => c.GetTextAsync()).ReturnsAsync(testContent);

        var historyManager = CreateTestHistoryManager();
        var manager = new ClipboardManager(mockClipboard.Object, historyManager, "test-platform");

        var originalOut = Console.Out;
        try
        {
            // Redirect stdout
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            var result = await manager.PasteToStdoutAsync();

            // Assert
            Assert.True(result);
            Assert.Equal(testContent, stringWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task PasteToStdout_WithEmptyClipboard_ReturnsFalse()
    {
        // Arrange
        var mockClipboard = new Mock<IClipboardProvider>();
        mockClipboard.Setup(c => c.GetTextAsync()).ReturnsAsync(string.Empty);

        var historyManager = CreateTestHistoryManager();
        var manager = new ClipboardManager(mockClipboard.Object, historyManager, "test-platform");

        var originalErr = Console.Error;
        try
        {
            // Redirect stderr to capture error message
            using var stringWriter = new StringWriter();
            Console.SetError(stringWriter);

            // Act
            var result = await manager.PasteToStdoutAsync();

            // Assert
            Assert.False(result);
        }
        finally
        {
            Console.SetError(originalErr);
        }
    }
}
