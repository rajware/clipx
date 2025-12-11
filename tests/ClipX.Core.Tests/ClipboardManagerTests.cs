using ClipX.Core.Interfaces;
using ClipX.Core.Services;
using Moq;
using Xunit;

namespace ClipX.Core.Tests;

public class ClipboardManagerTests
{
    [Fact]
    public async Task CopyFromStdin_WithValidInput_ReturnsTrue()
    {
        // Arrange
        var mockClipboard = new Mock<IClipboardProvider>();
        var historyManager = new HistoryManager();
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

        var historyManager = new HistoryManager();
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

        var historyManager = new HistoryManager();
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
