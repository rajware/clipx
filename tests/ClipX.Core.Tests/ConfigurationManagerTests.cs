using ClipX.Core.Interfaces;
using ClipX.Core.Models;
using ClipX.Core.Services;
using Moq;
using Xunit;

namespace ClipX.Core.Tests;

public class ConfigurationManagerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly Mock<IFileSystemProvider> _mockFileSystem;
    private readonly ConfigurationManager _configManager;

    public ConfigurationManagerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"clipx-config-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _mockFileSystem = new Mock<IFileSystemProvider>();
        _mockFileSystem.Setup(f => f.GetConfigDirectory()).Returns(_testDirectory);
        _mockFileSystem.Setup(f => f.EnsureDirectoryExists(It.IsAny<string>()))
            .Callback<string>(dir => Directory.CreateDirectory(dir));

        _configManager = new ConfigurationManager(_mockFileSystem.Object);
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
    public async Task LoadConfigAsync_WithNoFile_ReturnsDefaultConfig()
    {
        // Act
        var config = await _configManager.LoadConfigAsync();

        // Assert
        Assert.NotNull(config);
        Assert.Equal(25, config.MaxHistorySize);
        Assert.True(config.EnableDeduplication);
    }

    [Fact]
    public async Task LoadConfigAsync_WithNoFile_CreatesConfigFile()
    {
        // Act
        await _configManager.LoadConfigAsync();

        // Assert
        var configFile = Path.Combine(_testDirectory, "config.json");
        Assert.True(File.Exists(configFile));
    }

    [Fact]
    public async Task SaveConfigAsync_CreatesFile()
    {
        // Arrange
        var config = new ClipXConfiguration
        {
            MaxHistorySize = 50,
            EnableDeduplication = false
        };

        // Act
        await _configManager.SaveConfigAsync(config);

        // Assert
        var configFile = Path.Combine(_testDirectory, "config.json");
        Assert.True(File.Exists(configFile));
    }

    [Fact]
    public async Task SaveAndLoadConfig_PreservesValues()
    {
        // Arrange
        var originalConfig = new ClipXConfiguration
        {
            MaxHistorySize = 100,
            EnableDeduplication = false
        };

        // Act
        await _configManager.SaveConfigAsync(originalConfig);
        
        // Create new instance to test loading from file
        var newConfigManager = new ConfigurationManager(_mockFileSystem.Object);
        var loadedConfig = await newConfigManager.LoadConfigAsync();

        // Assert
        Assert.Equal(100, loadedConfig.MaxHistorySize);
        Assert.False(loadedConfig.EnableDeduplication);
    }

    [Fact]
    public async Task LoadConfigAsync_CachesConfig()
    {
        // Act
        var config1 = await _configManager.LoadConfigAsync();
        var config2 = await _configManager.LoadConfigAsync();

        // Assert
        Assert.Same(config1, config2); // Should be the same instance (cached)
    }

    [Fact]
    public async Task SaveConfigAsync_UpdatesCache()
    {
        // Arrange
        var config1 = await _configManager.LoadConfigAsync();
        var newConfig = new ClipXConfiguration
        {
            MaxHistorySize = 75,
            EnableDeduplication = true
        };

        // Act
        await _configManager.SaveConfigAsync(newConfig);
        var config2 = await _configManager.LoadConfigAsync();

        // Assert
        Assert.NotSame(config1, config2);
        Assert.Equal(75, config2.MaxHistorySize);
    }
}
