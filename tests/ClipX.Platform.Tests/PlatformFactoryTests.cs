using ClipX.Platform;
using Xunit;

namespace ClipX.Platform.Tests;

public class PlatformFactoryTests
{
    [Fact]
    public void CreateClipboardProvider_ReturnsNonNull()
    {
        // Act
        var provider = PlatformFactory.CreateClipboardProvider();

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void CreateFileSystemProvider_ReturnsNonNull()
    {
        // Act
        var provider = PlatformFactory.CreateFileSystemProvider();

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void GetPlatformIdentifier_ReturnsValidFormat()
    {
        // Act
        var identifier = PlatformFactory.GetPlatformIdentifier();

        // Assert
        Assert.NotNull(identifier);
        Assert.Contains("-", identifier);
        Assert.Matches(@"^(windows|macos|linux|unknown)-(x64|arm64|x86|arm)$", identifier);
    }
}
