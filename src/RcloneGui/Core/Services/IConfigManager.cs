using RcloneGui.Core.Models;
using RcloneGui.Features.Sftp.Models;
using RcloneGui.Features.Ftp.Models;

namespace RcloneGui.Core.Services;

/// <summary>
/// Interface for configuration management.
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    AppSettings? Settings { get; }

    /// <summary>
    /// Initializes the configuration manager.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    Task SaveSettingsAsync();

    /// <summary>
    /// Adds a new SFTP connection.
    /// </summary>
    Task AddConnectionAsync(SftpConnection connection);

    /// <summary>
    /// Updates an existing SFTP connection.
    /// </summary>
    Task UpdateConnectionAsync(SftpConnection connection);

    /// <summary>
    /// Deletes an SFTP connection.
    /// </summary>
    Task DeleteConnectionAsync(string connectionId);

    /// <summary>
    /// Gets an SFTP connection by ID.
    /// </summary>
    SftpConnection? GetConnection(string connectionId);

    /// <summary>
    /// Gets all configured SFTP connections.
    /// </summary>
    IReadOnlyList<SftpConnection> GetConnections();

    /// <summary>
    /// Adds a new FTP connection.
    /// </summary>
    Task AddFtpConnectionAsync(FtpConnection connection);

    /// <summary>
    /// Updates an existing FTP connection.
    /// </summary>
    Task UpdateFtpConnectionAsync(FtpConnection connection);

    /// <summary>
    /// Deletes an FTP connection.
    /// </summary>
    Task DeleteFtpConnectionAsync(string connectionId);

    /// <summary>
    /// Gets an FTP connection by ID.
    /// </summary>
    FtpConnection? GetFtpConnection(string connectionId);

    /// <summary>
    /// Gets all configured FTP connections.
    /// </summary>
    IReadOnlyList<FtpConnection> GetFtpConnections();

    /// <summary>
    /// Exports configuration to a file.
    /// </summary>
    Task<bool> ExportConfigAsync(string filePath, bool includePasswords = false);

    /// <summary>
    /// Imports configuration from a file.
    /// </summary>
    Task<(bool Success, string Message)> ImportConfigAsync(string filePath);

    /// <summary>
    /// Gets the path to the app data directory.
    /// </summary>
    string AppDataPath { get; }

    /// <summary>
    /// Gets the path to the rclone config file.
    /// </summary>
    string RcloneConfigPath { get; }
}
