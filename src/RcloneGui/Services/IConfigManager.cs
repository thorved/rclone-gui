using RcloneGui.Models;

namespace RcloneGui.Services;

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
    /// Updates an existing connection.
    /// </summary>
    Task UpdateConnectionAsync(SftpConnection connection);

    /// <summary>
    /// Deletes a connection.
    /// </summary>
    Task DeleteConnectionAsync(string connectionId);

    /// <summary>
    /// Gets a connection by ID.
    /// </summary>
    SftpConnection? GetConnection(string connectionId);

    /// <summary>
    /// Gets all configured connections.
    /// </summary>
    IReadOnlyList<SftpConnection> GetConnections();

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
