using System.Text.Json;
using System.Text.Json.Serialization;
using RcloneGui.Models;

namespace RcloneGui.Services;

/// <summary>
/// Manages application configuration and rclone config file.
/// </summary>
public class ConfigManager : IConfigManager
{
    private readonly string _appDataPath;
    private readonly string _settingsFilePath;
    private readonly string _rcloneConfigPath;
    private AppSettings? _settings;
    private readonly SemaphoreSlim _saveLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppSettings? Settings => _settings;
    public string AppDataPath => _appDataPath;
    public string RcloneConfigPath => _rcloneConfigPath;

    public ConfigManager()
    {
        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RcloneGui"
        );
        
        _settingsFilePath = Path.Combine(_appDataPath, "settings.json");
        _rcloneConfigPath = Path.Combine(_appDataPath, "rclone.conf");
        
        // Set RCLONE_CONFIG environment variable
        Environment.SetEnvironmentVariable("RCLONE_CONFIG", _rcloneConfigPath);
    }

    public async Task InitializeAsync()
    {
        // Ensure directories exist
        Directory.CreateDirectory(_appDataPath);
        
        // Create cache directory
        var cacheDir = Path.Combine(_appDataPath, "cache");
        Directory.CreateDirectory(cacheDir);
        Environment.SetEnvironmentVariable("RCLONE_CACHE_DIR", cacheDir);

        // Load or create settings
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            }
            catch
            {
                _settings = new AppSettings();
            }
        }
        
        _settings ??= new AppSettings();
        
        // Ensure rclone config file exists
        if (!File.Exists(_rcloneConfigPath))
        {
            await File.WriteAllTextAsync(_rcloneConfigPath, "# Rclone config managed by RcloneGui\n");
        }
    }

    public async Task SaveSettingsAsync()
    {
        if (_settings == null) return;
        
        await _saveLock.WaitAsync();
        try
        {
            _settings.Connections.ForEach(c => c.ModifiedAt = DateTime.UtcNow);
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    public async Task AddConnectionAsync(SftpConnection connection)
    {
        if (_settings == null) return;
        
        connection.CreatedAt = DateTime.UtcNow;
        connection.ModifiedAt = DateTime.UtcNow;
        _settings.Connections.Add(connection);
        await SaveSettingsAsync();
    }

    public async Task UpdateConnectionAsync(SftpConnection connection)
    {
        if (_settings == null) return;
        
        var index = _settings.Connections.FindIndex(c => c.Id == connection.Id);
        if (index >= 0)
        {
            connection.ModifiedAt = DateTime.UtcNow;
            _settings.Connections[index] = connection;
            await SaveSettingsAsync();
        }
    }

    public async Task DeleteConnectionAsync(string connectionId)
    {
        if (_settings == null) return;
        
        _settings.Connections.RemoveAll(c => c.Id == connectionId);
        await SaveSettingsAsync();
    }

    public SftpConnection? GetConnection(string connectionId)
    {
        return _settings?.Connections.FirstOrDefault(c => c.Id == connectionId);
    }

    public IReadOnlyList<SftpConnection> GetConnections()
    {
        return _settings?.Connections.AsReadOnly() ?? new List<SftpConnection>().AsReadOnly();
    }

    public async Task<bool> ExportConfigAsync(string filePath, bool includePasswords = false)
    {
        if (_settings == null) return false;

        try
        {
            var exportData = new ExportData
            {
                ExportVersion = 1,
                ExportDate = DateTime.UtcNow,
                AppSettings = new AppSettings
                {
                    StartMinimized = _settings.StartMinimized,
                    MinimizeToTray = _settings.MinimizeToTray,
                    StartWithWindows = _settings.StartWithWindows,
                    AutoMountOnStartup = _settings.AutoMountOnStartup,
                    ShowNotifications = _settings.ShowNotifications,
                    Theme = _settings.Theme,
                    CacheDirectory = _settings.CacheDirectory,
                    Connections = _settings.Connections.Select(c => new SftpConnection
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Host = c.Host,
                        Port = c.Port,
                        Username = c.Username,
                        AuthType = c.AuthType,
                        ObscuredPassword = includePasswords ? c.ObscuredPassword : null,
                        KeyFilePath = c.KeyFilePath,
                        ObscuredKeyPassphrase = includePasswords ? c.ObscuredKeyPassphrase : null,
                        RemotePath = c.RemotePath,
                        MountSettings = c.MountSettings,
                        AutoMount = c.AutoMount,
                        CreatedAt = c.CreatedAt,
                        ModifiedAt = c.ModifiedAt
                    }).ToList()
                }
            };

            var json = JsonSerializer.Serialize(exportData, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string Message)> ImportConfigAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return (false, "File not found");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var exportData = JsonSerializer.Deserialize<ExportData>(json, JsonOptions);

            if (exportData?.AppSettings == null)
            {
                return (false, "Invalid configuration file format");
            }

            // Merge settings
            if (_settings != null)
            {
                _settings.StartMinimized = exportData.AppSettings.StartMinimized;
                _settings.MinimizeToTray = exportData.AppSettings.MinimizeToTray;
                _settings.StartWithWindows = exportData.AppSettings.StartWithWindows;
                _settings.AutoMountOnStartup = exportData.AppSettings.AutoMountOnStartup;
                _settings.ShowNotifications = exportData.AppSettings.ShowNotifications;
                _settings.Theme = exportData.AppSettings.Theme;

                // Add connections that don't exist
                foreach (var connection in exportData.AppSettings.Connections)
                {
                    var existing = _settings.Connections.FirstOrDefault(c => c.Name == connection.Name);
                    if (existing == null)
                    {
                        connection.Id = Guid.NewGuid().ToString(); // New ID for imported connection
                        _settings.Connections.Add(connection);
                    }
                }

                await SaveSettingsAsync();
            }

            var importedCount = exportData.AppSettings.Connections.Count;
            return (true, $"Successfully imported {importedCount} connection(s)");
        }
        catch (Exception ex)
        {
            return (false, $"Import failed: {ex.Message}");
        }
    }

    private class ExportData
    {
        public int ExportVersion { get; set; }
        public DateTime ExportDate { get; set; }
        public AppSettings? AppSettings { get; set; }
    }
}
