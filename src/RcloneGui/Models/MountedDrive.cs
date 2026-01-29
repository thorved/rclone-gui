using System.Diagnostics;

namespace RcloneGui.Models;

/// <summary>
/// Represents an active mount with its associated process.
/// </summary>
public class MountedDrive
{
    /// <summary>
    /// The connection that is mounted (SftpConnection or FtpConnection).
    /// </summary>
    public required object Connection { get; init; }

    /// <summary>
    /// The drive letter where the connection is mounted.
    /// </summary>
    public required string DriveLetter { get; init; }

    /// <summary>
    /// Current status of the mount.
    /// </summary>
    public MountStatus Status { get; set; } = MountStatus.Unmounted;

    /// <summary>
    /// The rclone process handling this mount.
    /// </summary>
    public Process? RcloneProcess { get; set; }

    /// <summary>
    /// Time when the mount was established.
    /// </summary>
    public DateTime? MountedAt { get; set; }

    /// <summary>
    /// Last error message if mount failed.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Process ID of the rclone mount process.
    /// </summary>
    public int? ProcessId => RcloneProcess?.Id;

    /// <summary>
    /// Gets the full drive path (e.g., "X:\").
    /// </summary>
    public string DrivePath => $"{DriveLetter}:\\";

    /// <summary>
    /// Gets the display name for the mount.
    /// </summary>
    public string DisplayName
    {
        get
        {
            var mountSettings = Connection switch
            {
                SftpConnection sftp => sftp.MountSettings,
                FtpConnection ftp => ftp.MountSettings,
                _ => null
            };
            var name = Connection switch
            {
                SftpConnection sftp => sftp.Name,
                FtpConnection ftp => ftp.Name,
                _ => "Unknown"
            };
            return mountSettings?.VolumeName ?? name;
        }
    }

    /// <summary>
    /// Gets the connection ID.
    /// </summary>
    public string ConnectionId => Connection switch
    {
        SftpConnection sftp => sftp.Id,
        FtpConnection ftp => ftp.Id,
        _ => throw new InvalidOperationException("Unknown connection type")
    };

    /// <summary>
    /// Gets the connection name.
    /// </summary>
    public string ConnectionName => Connection switch
    {
        SftpConnection sftp => sftp.Name,
        FtpConnection ftp => ftp.Name,
        _ => "Unknown"
    };
}

/// <summary>
/// Status of a mount operation.
/// </summary>
public enum MountStatus
{
    /// <summary>Drive is not mounted.</summary>
    Unmounted,
    
    /// <summary>Mount operation in progress.</summary>
    Mounting,
    
    /// <summary>Drive is mounted and accessible.</summary>
    Mounted,
    
    /// <summary>Unmount operation in progress.</summary>
    Unmounting,
    
    /// <summary>Mount failed with an error.</summary>
    Error
}
