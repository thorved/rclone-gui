using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RcloneGui.Core.Models;
using RcloneGui.Core.Services;

namespace RcloneGui.Features.Dashboard.ViewModels;

/// <summary>
/// ViewModel for settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigManager _configManager;
    private readonly IWinFspManager _winFspManager;
    private readonly ISftpService _sftpService;
    private bool _isInitializing;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _minimizeToTray;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _autoMountOnStartup;

    [ObservableProperty]
    private bool _unmountOnClose;

    [ObservableProperty]
    private bool _showNotifications;

    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private AppTheme _theme;

    [ObservableProperty]
    private string? _customRclonePath;

    [ObservableProperty]
    private string? _cacheDirectory;

    [ObservableProperty]
    private bool _isWinFspInstalled;

    [ObservableProperty]
    private string? _winFspVersion;

    [ObservableProperty]
    private string? _rcloneVersion;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isBusy;

    public IReadOnlyList<AppTheme> Themes { get; } = Enum.GetValues<AppTheme>().ToList();

    public SettingsViewModel(IConfigManager configManager, IWinFspManager winFspManager, ISftpService sftpService)
    {
        _configManager = configManager;
        _winFspManager = winFspManager;
        _sftpService = sftpService;
    }

    public async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            var settings = _configManager.Settings;
            if (settings != null)
            {
                StartMinimized = settings.StartMinimized;
                MinimizeToTray = settings.MinimizeToTray;
                StartWithWindows = settings.StartWithWindows;
                AutoMountOnStartup = settings.AutoMountOnStartup;
                UnmountOnClose = settings.UnmountOnClose;
                ShowNotifications = settings.ShowNotifications;
                Theme = settings.Theme;
                SelectedThemeIndex = (int)settings.Theme;
                CustomRclonePath = settings.CustomRclonePath;
                CacheDirectory = settings.CacheDirectory;
            }

            IsWinFspInstalled = _winFspManager.IsInstalled();
            WinFspVersion = _winFspManager.GetVersion();
            RcloneVersion = await _sftpService.GetVersionAsync();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    public Task LoadSettingsAsync() => InitializeAsync();

    [RelayCommand]
    private async Task ExportConfigAsync()
    {
        // Note: In actual implementation, this would use a file picker dialog
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"rclonegui_config_{timestamp}.json";
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var filePath = Path.Combine(documentsPath, fileName);

        await ExportConfigToPathAsync(filePath);
    }

    public async Task ExportConfigToPathAsync(string filePath)
    {
        IsBusy = true;
        try
        {
            var success = await _configManager.ExportConfigAsync(filePath, includePasswords: false);
            StatusMessage = success 
                ? $"Configuration exported to: {filePath}" 
                : "Failed to export configuration";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportConfigAsync(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            StatusMessage = "No file selected";
            return;
        }

        await ImportConfigFromPathAsync(filePath);
    }

    public async Task ImportConfigFromPathAsync(string filePath)
    {
        IsBusy = true;
        try
        {
            var (success, message) = await _configManager.ImportConfigAsync(filePath);
            StatusMessage = message;
            
            // Reload settings after import
            if (success)
            {
                await LoadSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task InstallWinFspAsync()
    {
        IsBusy = true;
        StatusMessage = "Installing WinFsp...";

        try
        {
            var (success, message) = await _winFspManager.InstallAsync();
            StatusMessage = message;
            
            if (success)
            {
                IsWinFspInstalled = true;
                WinFspVersion = _winFspManager.GetVersion();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenWinFspDownload()
    {
        var url = _winFspManager.GetDownloadUrl();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void OpenCacheDirectory()
    {
        var cacheDir = CacheDirectory ?? Path.Combine(_configManager.AppDataPath, "cache");
        if (Directory.Exists(cacheDir))
        {
            System.Diagnostics.Process.Start("explorer.exe", cacheDir);
        }
    }

    private void UpdateWindowsStartup(bool enable)
    {
        const string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string appName = "RcloneGui";

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(registryKey, true);
            if (key != null)
            {
                if (enable)
                {
                    var exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // Add --minimized flag if StartMinimized is enabled
                        var args = StartMinimized ? " --minimized" : "";
                        key.SetValue(appName, $"\"{exePath}\"{args}");
                    }
                }
                else
                {
                    key.DeleteValue(appName, false);
                }
            }
        }
        catch
        {
            // Ignore registry errors
        }
    }

    // Auto-save handlers for toggle switches
    partial void OnStartWithWindowsChanged(bool value)
    {
        if (_isInitializing) return;
        UpdateWindowsStartup(value);
        _ = AutoSaveAsync();
    }

    partial void OnStartMinimizedChanged(bool value)
    {
        if (_isInitializing) return;
        // Update registry if StartWithWindows is enabled
        if (StartWithWindows)
        {
            UpdateWindowsStartup(true);
        }
        _ = AutoSaveAsync();
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        if (!_isInitializing) _ = AutoSaveAsync();
    }

    partial void OnAutoMountOnStartupChanged(bool value)
    {
        if (!_isInitializing) _ = AutoSaveAsync();
    }

    partial void OnUnmountOnCloseChanged(bool value)
    {
        if (!_isInitializing) _ = AutoSaveAsync();
    }

    partial void OnShowNotificationsChanged(bool value)
    {
        if (!_isInitializing) _ = AutoSaveAsync();
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        Theme = (AppTheme)value;
        if (!_isInitializing)
        {
            ApplyTheme(Theme);
            _ = AutoSaveAsync();
        }
    }

    private async Task AutoSaveAsync()
    {
        // Don't save during initialization - properties are being loaded from storage
        if (_isInitializing) return;
        
        try
        {
            var settings = _configManager.Settings;
            if (settings != null)
            {
                settings.StartMinimized = StartMinimized;
                settings.MinimizeToTray = MinimizeToTray;
                settings.StartWithWindows = StartWithWindows;
                settings.AutoMountOnStartup = AutoMountOnStartup;
                settings.UnmountOnClose = UnmountOnClose;
                settings.ShowNotifications = ShowNotifications;
                settings.Theme = Theme;
                settings.CustomRclonePath = CustomRclonePath;
                settings.CacheDirectory = CacheDirectory;

                await _configManager.SaveSettingsAsync();
            }
        }
        catch
        {
            // Ignore auto-save errors
        }
    }

    private void ApplyTheme(AppTheme theme)
    {
        // Notify App to apply theme change
        App.ApplyTheme(theme);
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        IsBusy = true;
        StatusMessage = "Clearing cache...";

        try
        {
            var cacheDir = CacheDirectory ?? Path.Combine(_configManager.AppDataPath, "cache");
            if (Directory.Exists(cacheDir))
            {
                var dirInfo = new DirectoryInfo(cacheDir);
                long totalSize = 0;
                
                // Calculate size before deletion
                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    totalSize += file.Length;
                }

                // Delete all files and subdirectories
                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    try { file.Delete(); } catch { }
                }
                foreach (var dir in dirInfo.GetDirectories())
                {
                    try { dir.Delete(true); } catch { }
                }

                var sizeMb = totalSize / (1024.0 * 1024.0);
                StatusMessage = $"Cache cleared. Freed {sizeMb:F2} MB";
            }
            else
            {
                StatusMessage = "Cache directory is empty";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to clear cache: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }

        await Task.CompletedTask;
    }
}
