using RcloneGui.Core.Models;
using RcloneGui.Features.Ftp.Models;

namespace RcloneGui.Core.Services;

/// <summary>
/// Interface for FTP-specific rclone operations.
/// </summary>
public interface IFtpService
{
    /// <summary>
    /// Gets the path to the rclone executable.
    /// </summary>
    string RclonePath { get; }

    /// <summary>
    /// Gets the rclone version.
    /// </summary>
    Task<string> GetVersionAsync();

    /// <summary>
    /// Obscures a password for secure storage in rclone config.
    /// </summary>
    Task<string> ObscurePasswordAsync(string password);

    /// <summary>
    /// Creates an FTP remote configuration in rclone.
    /// </summary>
    Task<bool> CreateRemoteAsync(FtpConnection connection);

    /// <summary>
    /// Deletes a remote configuration from rclone.
    /// </summary>
    Task<bool> DeleteRemoteAsync(string remoteName);

    /// <summary>
    /// Tests connection to an FTP server.
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(FtpConnection connection);

    /// <summary>
    /// Mounts an FTP remote to a drive letter.
    /// </summary>
    Task<MountResult> MountAsync(FtpConnection connection, string driveLetter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unmounts a drive.
    /// </summary>
    Task<bool> UnmountAsync(string driveLetter);

    /// <summary>
    /// Gets list of available (unused) drive letters.
    /// </summary>
    List<string> GetAvailableDriveLetters();
}
