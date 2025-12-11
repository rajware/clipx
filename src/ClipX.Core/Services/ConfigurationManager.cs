using System.Text.Json;
using ClipX.Core.Interfaces;
using ClipX.Core.Models;

namespace ClipX.Core.Services;

/// <summary>
/// Manages application configuration.
/// </summary>
public class ConfigurationManager
{
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly string _configFilePath;
    private ClipXConfiguration? _cachedConfig;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ConfigurationManager(IFileSystemProvider fileSystemProvider)
    {
        _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
        
        var configDir = _fileSystemProvider.GetConfigDirectory();
        _fileSystemProvider.EnsureDirectoryExists(configDir);
        _configFilePath = Path.Combine(configDir, "config.json");
    }

    /// <summary>
    /// Loads configuration from file, or returns default configuration if file doesn't exist.
    /// </summary>
    public async Task<ClipXConfiguration> LoadConfigAsync()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        if (!File.Exists(_configFilePath))
        {
            _cachedConfig = new ClipXConfiguration();
            // Save default configuration
            await SaveConfigAsync(_cachedConfig);
            return _cachedConfig;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            _cachedConfig = JsonSerializer.Deserialize<ClipXConfiguration>(json) ?? new ClipXConfiguration();
            return _cachedConfig;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves configuration to file.
    /// </summary>
    public async Task SaveConfigAsync(ClipXConfiguration config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
            _cachedConfig = config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
        }
    }
}
