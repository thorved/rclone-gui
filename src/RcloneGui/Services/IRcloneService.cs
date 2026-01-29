using RcloneGui.Models;

namespace RcloneGui.Services;

/// <summary>
/// Interface for rclone command execution.
/// </summary>
public interface IRcloneService
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
    /// Creates an SFTP remote configuration in rclone.
    /// </summary>
    Task<bool> CreateSftpRemoteAsync(SftpConnection connection);

    /// <summary>
    /// Creates an FTP remote configuration in rclone.
    /// </summary>
    Task<bool> CreateFtpRemoteAsync(FtpConnection connection);

    /// <summary>
    /// Deletes a remote configuration from rclone.
    /// </summary>
    Task<bool> DeleteRemoteAsync(string remoteName);

    /// <summary>
    /// Tests connection to an SFTP server.
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(SftpConnection connection);

    /// <summary>
    /// Tests connection to an FTP server.
    /// </summary>
    Task<(bool Success, string Message)> TestFtpConnectionAsync(FtpConnection connection);

    /// <summary>
    /// Lists all configured remotes.
    /// </summary>
    Task<List<string>> ListRemotesAsync();

    /// <summary>
    /// Mounts an SFTP remote to a drive letter.
    /// </summary>
    Task<MountResult> MountAsync(SftpConnection connection, string driveLetter, CancellationToken cancellationToken = default);

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

/// <summary>
/// Result of a mount operation.
/// </summary>
public class MountResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public System.Diagnostics.Process? Process { get; init; }
    public string? DriveLetter { get; init; }
}
