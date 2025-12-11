using System.Security.Cryptography;
using System.Text;

namespace ClipX.Core.Models;

/// <summary>
/// Represents a clipboard history entry.
/// </summary>
public class ClipboardEntry
{
    /// <summary>
    /// Unique identifier for this entry.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The clipboard text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this entry was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source platform/device identifier.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Size of the content in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// SHA256 hash of the content for deduplication.
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new clipboard entry from the given content.
    /// </summary>
    /// <param name="content">The clipboard content.</param>
    /// <param name="source">The source platform/device identifier.</param>
    /// <returns>A new ClipboardEntry instance.</returns>
    public static ClipboardEntry Create(string content, string source)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(contentBytes);
        var hashString = Convert.ToHexString(hash).ToLowerInvariant();

        return new ClipboardEntry
        {
            Content = content,
            Source = source,
            SizeBytes = contentBytes.Length,
            ContentHash = hashString
        };
    }
}
