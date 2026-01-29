using System.Text.Json.Serialization;

namespace RcloneGui.Models;

/// <summary>
/// Represents an SFTP connection configuration.
/// </summary>
public class SftpConnection
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
    /// SFTP server hostname or IP address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SFTP server port (default: 22).
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Authentication type: Password or KeyFile.
    /// </summary>
    public AuthenticationType AuthType { get; set; } = AuthenticationType.Password;

    /// <summary>
    /// Obscured password (for password auth). Use rclone obscure to encode.
    /// </summary>
    public string? ObscuredPassword { get; set; }

    /// <summary>
    /// Path to SSH private key file (for key auth).
    /// </summary>
    public string? KeyFilePath { get; set; }

    /// <summary>
    /// Passphrase for encrypted private key (obscured).
    /// </summary>
    public string? ObscuredKeyPassphrase { get; set; }

    /// <summary>
    /// Remote path on the SFTP server to mount.
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
    /// Gets the rclone remote name (used in rclone config).
    /// </summary>
    [JsonIgnore]
    public string RcloneRemoteName => $"sftp_{Id.Replace("-", "")}";
}

/// <summary>
/// Authentication type for SFTP connection.
/// </summary>
public enum AuthenticationType
{
    Password,
    KeyFile
}

/// <summary>
/// Mount-specific settings for a connection.
/// </summary>
public class MountSettings
{
    /// <summary>
    /// Preferred drive letter (A-Z) or empty for auto-assign.
    /// </summary>
    public string DriveLetter { get; set; } = string.Empty;

    /// <summary>
    /// Whether to mount as network drive instead of fixed disk.
    /// </summary>
    public bool NetworkMode { get; set; } = true;

    /// <summary>
    /// Custom volume name displayed in Explorer.
    /// </summary>
    public string? VolumeName { get; set; }

    /// <summary>
    /// Whether to mount as read-only.
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// VFS cache mode: off, minimal, writes, full.
    /// </summary>
    public VfsCacheMode CacheMode { get; set; } = VfsCacheMode.Writes;

    /// <summary>
    /// Maximum VFS cache size (e.g., "10G", "500M").
    /// </summary>
    public string CacheMaxSize { get; set; } = "10G";

    /// <summary>
    /// Directory cache time in minutes.
    /// </summary>
    public int DirCacheTimeMinutes { get; set; } = 5;
}

/// <summary>
/// VFS cache mode options.
/// </summary>
public enum VfsCacheMode
{
    Off,
    Minimal,
    Writes,
    Full
}
