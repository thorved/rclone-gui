using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Models;
using RcloneGui.Services;
using System.Collections.ObjectModel;

namespace RcloneGui.ViewModels;

/// <summary>
/// ViewModel for the main window.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IConfigManager _configManager;
    private readonly IMountManager _mountManager;
    private readonly IWinFspManager _winFspManager;
    private readonly IRcloneService _rcloneService;

    [ObservableProperty]
    private ObservableCollection<DriveItemViewModel> _drives = new();

    [ObservableProperty]
    private DriveItemViewModel? _selectedDrive;

    [ObservableProperty]
    private bool _isWinFspInstalled;

    [ObservableProperty]
    private string _rcloneVersion = "Unknown";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    public MainViewModel(
        IConfigManager configManager,
        IMountManager mountManager,
        IWinFspManager winFspManager,
        IRcloneService rcloneService)
    {
        _configManager = configManager;
        _mountManager = mountManager;
        _winFspManager = winFspManager;
        _rcloneService = rcloneService;

        _mountManager.MountStatusChanged += OnMountStatusChanged;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            // Check WinFsp
            IsWinFspInstalled = _winFspManager.IsInstalled();

            // Get rclone version
            RcloneVersion = await _rcloneService.GetVersionAsync();

            // Load connections
            await RefreshDrivesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDrivesAsync()
    {
        Drives.Clear();

        var connections = _configManager.GetConnections();
        foreach (var connection in connections)
        {
            var mountStatus = _mountManager.GetMountStatus(connection.Id);
            var driveVm = new DriveItemViewModel(connection, _mountManager, _rcloneService)
            {
                Status = mountStatus?.Status ?? MountStatus.Unmounted,
                DriveLetter = mountStatus?.DriveLetter ?? connection.MountSettings.DriveLetter
            };
            Drives.Add(driveVm);
        }
    }

    [RelayCommand]
    private async Task MountAllAsync()
    {
        IsLoading = true;
        StatusMessage = "Mounting all drives...";

        try
        {
            foreach (var drive in Drives.Where(d => d.Status == MountStatus.Unmounted))
            {
                await drive.MountCommand.ExecuteAsync(null);
            }
            StatusMessage = "All drives mounted";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UnmountAllAsync()
    {
        IsLoading = true;
        StatusMessage = "Unmounting all drives...";

        try
        {
            await _mountManager.UnmountAllAsync();
            StatusMessage = "All drives unmounted";
            await RefreshDrivesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task InstallWinFspAsync()
    {
        var result = await _winFspManager.InstallAsync();
        StatusMessage = result.Message;
        IsWinFspInstalled = _winFspManager.IsInstalled();
    }

    [RelayCommand]
    private async Task DeleteConnectionAsync(DriveItemViewModel? drive)
    {
        if (drive == null) return;

        // Unmount if mounted
        if (drive.Status == MountStatus.Mounted)
        {
            await _mountManager.UnmountAsync(drive.Connection.Id);
        }

        // Delete from rclone config
        await _rcloneService.DeleteRemoteAsync(drive.Connection.RcloneRemoteName);

        // Delete from app config
        await _configManager.DeleteConnectionAsync(drive.Connection.Id);

        // Refresh UI
        await RefreshDrivesAsync();
    }

    private void OnMountStatusChanged(object? sender, MountStatusChangedEventArgs e)
    {
        var drive = Drives.FirstOrDefault(d => d.Connection.Id == e.ConnectionId);
        if (drive != null)
        {
            drive.Status = e.NewStatus;
            drive.DriveLetter = e.DriveLetter ?? drive.DriveLetter;
            if (e.NewStatus == MountStatus.Error)
            {
                drive.ErrorMessage = e.ErrorMessage;
            }
        }
    }
}
