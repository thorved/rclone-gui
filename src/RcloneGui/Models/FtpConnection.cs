using System.Text.Json.Serialization;

namespace RcloneGui.Models;

/// <summary>
/// Represents an FTP/FTPS connection configuration.
/// </summary>
public class FtpConnection
{
    /// <summary>
    /// Unique identifier for the connection.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the connection.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// FTP server hostname or IP address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// FTP server port (default: 21 for FTP, 990 for FTPS implicit).
    /// </summary>
    public int Port { get; set; } = 21;

    /// <summary>
    /// Username for authentication. Empty for anonymous FTP.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Obscured password (empty for anonymous FTP). Use rclone obscure to encode.
    /// </summary>
    public string? ObscuredPassword { get; set; }

    /// <summary>
    /// TLS/SSL mode for secure FTP (FTPS).
    /// </summary>
    public FtpTlsMode TlsMode { get; set; } = FtpTlsMode.None;

    /// <summary>
    /// Use passive mode for data connections (default: true).
    /// </summary>
    public bool PassiveMode { get; set; } = true;

    /// <summary>
    /// Remote path on the FTP server to mount.
    /// </summary>
    public string RemotePath { get; set; } = "/";

    /// <summary>
    /// Mount settings for this connection.
    /// </summary>
    public MountSettings MountSettings { get; set; } = new();

    /// <summary>
    /// Whether to auto-mount this connection on app startup.
    /// </summary>
    public bool AutoMount { get; set; } = false;

    /// <summary>
    /// Date when connection was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when connection was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether this is an anonymous FTP connection.
    /// </summary>
    [JsonIgnore]
    public bool IsAnonymous => string.IsNullOrEmpty(Username) || Username.ToLower() == "anonymous";

    /// <summary>
    /// Gets the rclone remote name (used in rclone config).
    /// </summary>
    [JsonIgnore]
    public string RcloneRemoteName => $"ftp_{Id.Replace("-", "")}";
}

/// <summary>
/// TLS/SSL mode for FTP connections.
/// </summary>
public enum FtpTlsMode
{
    /// <summary>
    /// No encryption (plain FTP).
    /// </summary>
    None,

    /// <summary>
    /// Implicit TLS on port 990 (FTPS).
    /// </summary>
    Implicit,

    /// <summary>
    /// Explicit TLS via AUTH TLS command.
    /// </summary>
    Explicit
}
