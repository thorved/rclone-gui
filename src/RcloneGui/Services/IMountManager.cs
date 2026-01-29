using RcloneGui.Models;

namespace RcloneGui.Services;

/// <summary>
/// Interface for mount management.
/// </summary>
public interface IMountManager
{
    /// <summary>
    /// Gets all currently mounted drives.
    /// </summary>
    IReadOnlyList<MountedDrive> MountedDrives { get; }

    /// <summary>
    /// Event raised when mount status changes.
    /// </summary>
    event EventHandler<MountStatusChangedEventArgs>? MountStatusChanged;

    /// <summary>
    /// Mounts an SFTP connection to a drive.
    /// </summary>
    Task<MountedDrive> MountAsync(SftpConnection connection, string? preferredDriveLetter = null);

    /// <summary>
    /// Mounts an FTP connection to a drive.
    /// </summary>
    Task<MountedDrive> MountAsync(FtpConnection connection, string? preferredDriveLetter = null);

    /// <summary>
    /// Unmounts a drive.
    /// </summary>
    Task<bool> UnmountAsync(string connectionId);

    /// <summary>
    /// Unmounts all drives.
    /// </summary>
    Task UnmountAllAsync();

    /// <summary>
    /// Auto-mounts connections marked for auto-mount.
    /// </summary>
    Task AutoMountAsync();

    /// <summary>
    /// Gets the mount status for a connection.
    /// </summary>
    MountedDrive? GetMountStatus(string connectionId);

    /// <summary>
    /// Checks if a connection is currently mounted.
    /// </summary>
    bool IsMounted(string connectionId);
}

/// <summary>
/// Event args for mount status changes.
/// </summary>
public class MountStatusChangedEventArgs : EventArgs
{
    public required string ConnectionId { get; init; }
    public required MountStatus OldStatus { get; init; }
    public required MountStatus NewStatus { get; init; }
    public string? DriveLetter { get; init; }
    public string? ErrorMessage { get; init; }
}
