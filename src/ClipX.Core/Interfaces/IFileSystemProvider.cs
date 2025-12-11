namespace ClipX.Core.Interfaces;

/// <summary>
/// Provides platform-specific file system paths and operations.
/// </summary>
public interface IFileSystemProvider
{
    /// <summary>
    /// Gets the platform-appropriate directory for configuration files.
    /// </summary>
    /// <returns>Absolute path to the configuration directory.</returns>
    string GetConfigDirectory();

    /// <summary>
    /// Gets the platform-appropriate directory for application data.
    /// </summary>
    /// <returns>Absolute path to the data directory.</returns>
    string GetDataDirectory();

    /// <summary>
    /// Ensures that a directory exists, creating it if necessary.
    /// </summary>
    /// <param name="path">The directory path to ensure exists.</param>
    void EnsureDirectoryExists(string path);
}
