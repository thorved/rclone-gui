using RcloneGui.Core.Models;
using RcloneGui.Features.Sftp.Models;

namespace RcloneGui.Core.Services;

/// <summary>
/// Interface for SFTP-specific rclone operations.
/// </summary>
public interface ISftpService
{
    /// <summary>
    /// Gets the path to the rclone executable.
    /// </summary>    string RclonePath { get; }

    /// <summary>
    /// Gets the rclone version.
    /// </summary>
    Task<string> GetVersionAsync();

    /// <summary>
    /// Obscures a password for secure storage in rclone config.
    /// </summary>
    Task<string> ObscurePasswordAsync(string password);

    /// <summary>
    /// Creates an SFTP remote configuration in rclone.
    /// </summary>
    Task<bool> CreateRemoteAsync(SftpConnection connection);

    /// <summary>
    /// Deletes a remote configuration from rclone.
    /// </summary>
    Task<bool> DeleteRemoteAsync(string remoteName);

    /// <summary>
    /// Tests connection to an SFTP server.
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(SftpConnection connection);

    /// <summary>
    /// Mounts an SFTP remote to a drive letter.
    /// </summary>
    Task<MountResult> MountAsync(SftpConnection connection, string driveLetter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unmounts a drive.
    /// </summary>
    Task<bool> UnmountAsync(string driveLetter);

    /// <summary>
    /// Gets list of available (unused) drive letters.
    /// </summary>
    List<string> GetAvailableDriveLetters();
}
