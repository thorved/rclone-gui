using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RcloneGui.Models;
using RcloneGui.Services;

namespace RcloneGui.ViewModels;

/// <summary>
/// ViewModel for settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigManager _configManager;
    private readonly IWinFspManager _winFspManager;
    private readonly IRcloneService _rcloneService;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _minimizeToTray;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _autoMountOnStartup;

    [ObservableProperty]
    private bool _showNotifications;

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

    public SettingsViewModel(IConfigManager configManager, IWinFspManager winFspManager, IRcloneService rcloneService)
    {
        _configManager = configManager;
        _winFspManager = winFspManager;
        _rcloneService = rcloneService;
    }

    public async Task InitializeAsync()
    {
        var settings = _configManager.Settings;
        if (settings != null)
        {
            StartMinimized = settings.StartMinimized;
            MinimizeToTray = settings.MinimizeToTray;
            StartWithWindows = settings.StartWithWindows;
            AutoMountOnStartup = settings.AutoMountOnStartup;
            ShowNotifications = settings.ShowNotifications;
            Theme = settings.Theme;
            CustomRclonePath = settings.CustomRclonePath;
            CacheDirectory = settings.CacheDirectory;
        }

        IsWinFspInstalled = _winFspManager.IsInstalled();
        WinFspVersion = _winFspManager.GetVersion();
        RcloneVersion = await _rcloneService.GetVersionAsync();
    }

    public Task LoadSettingsAsync() => InitializeAsync();

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsBusy = true;
        StatusMessage = null;

        try
        {
            var settings = _configManager.Settings;
            if (settings != null)
            {
                settings.StartMinimized = StartMinimized;
                settings.MinimizeToTray = MinimizeToTray;
                settings.StartWithWindows = StartWithWindows;
                settings.AutoMountOnStartup = AutoMountOnStartup;
                settings.ShowNotifications = ShowNotifications;
                settings.Theme = Theme;
                settings.CustomRclonePath = CustomRclonePath;
                settings.CacheDirectory = CacheDirectory;

                await _configManager.SaveSettingsAsync();

                // Update Windows startup
                UpdateWindowsStartup(StartWithWindows);

                StatusMessage = "Settings saved successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportConfigAsync()
    {
        // Note: In actual implementation, this would use a file picker dialog
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"rclonegui_config_{timestamp}.json";
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var filePath = Path.Combine(documentsPath, fileName);

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

        IsBusy = true;
        try
        {
            var (success, message) = await _configManager.ImportConfigAsync(filePath);
            StatusMessage = message;
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
                        key.SetValue(appName, $"\"{exePath}\" --minimized");
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
}
