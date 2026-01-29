namespace RcloneGui.Core.Abstractions;

/// <summary>
/// Common interface for all connection types (SFTP, FTP, Google Drive, etc.)
/// </summary>
public interface IConnection
{
    string Id { get; set; }
    string Name { get; set; }
    bool AutoMount { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime ModifiedAt { get; set; }
    
    /// <summary>
    /// Gets the rclone remote name (used in rclone config).
    /// </summary>
    string RcloneRemoteName { get; }
    
    /// <summary>
    /// Gets the connection type identifier (e.g., "sftp", "ftp", "drive")
    /// </summary>
    string ConnectionType { get; }
}
