using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Core.Models;
using RcloneGui.Core.Services;
using RcloneGui.Features.Sftp.Models;
using RcloneGui.Features.Ftp.Models;
using System.Collections.ObjectModel;

namespace RcloneGui.Features.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the main window.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IConfigManager _configManager;
    private readonly IMountManager _mountManager;
    private readonly IWinFspManager _winFspManager;
    private readonly ISftpService _sftpService;
    private readonly IFtpService _ftpService;

    [ObservableProperty]
    private ObservableCollection<DriveItemViewModel> _drives = new();

    partial void OnDrivesChanged(ObservableCollection<DriveItemViewModel> value)
    {
        OnPropertyChanged(nameof(HasDrives));
        OnPropertyChanged(nameof(HasNoDrives));
    }

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

    public bool HasDrives => Drives.Count > 0;
    public bool HasNoDrives => Drives.Count == 0;

    public MainViewModel(
        IConfigManager configManager,
        IMountManager mountManager,
        IWinFspManager winFspManager,
        ISftpService sftpService,
        IFtpService ftpService)
    {
        _configManager = configManager;
        _mountManager = mountManager;
        _winFspManager = winFspManager;
        _sftpService = sftpService;
        _ftpService = ftpService;

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
            RcloneVersion = await _sftpService.GetVersionAsync();

            // Load connections
            await RefreshDrivesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RefreshDrivesAsync()
    {
        Drives.Clear();

        // Load SFTP connections
        var sftpConnections = _configManager.GetConnections();
        foreach (var connection in sftpConnections)
        {
            var mountStatus = _mountManager.GetMountStatus(connection.Id);
            var driveVm = new DriveItemViewModel(connection, _mountManager, _sftpService, _ftpService)
            {
                Status = mountStatus?.Status ?? MountStatus.Unmounted,
                DriveLetter = mountStatus?.DriveLetter ?? connection.MountSettings.DriveLetter
            };
            
            // Subscribe to events
            driveVm.EditRequested += OnDriveEditRequested;
            driveVm.DeleteRequested += OnDriveDeleteRequested;
            driveVm.DuplicateRequested += OnDriveDuplicateRequested;
            
            Drives.Add(driveVm);
        }

        // Load FTP connections
        var ftpConnections = _configManager.GetFtpConnections();
        foreach (var connection in ftpConnections)
        {
            var mountStatus = _mountManager.GetMountStatus(connection.Id);
            var driveVm = new DriveItemViewModel(connection, _mountManager, _sftpService, _ftpService)
            {
                Status = mountStatus?.Status ?? MountStatus.Unmounted,
                DriveLetter = mountStatus?.DriveLetter ?? connection.MountSettings.DriveLetter
            };
            
            // Subscribe to events
            driveVm.EditRequested += OnDriveEditRequested;
            driveVm.DeleteRequested += OnDriveDeleteRequested;
            driveVm.DuplicateRequested += OnDriveDuplicateRequested;
            
            Drives.Add(driveVm);
        }
    }

    private void OnDriveEditRequested(object? sender, EventArgs e)
    {
        if (sender is DriveItemViewModel drive)
        {
            if (drive.IsSftp && drive.SftpConnection != null)
            {
                // Navigate to SFTP edit - handled by MainWindow
            }
            else if (drive.IsFtp && drive.FtpConnection != null)
            {
                // Navigate to FTP edit - handled by MainWindow
            }
        }
    }

    private async void OnDriveDeleteRequested(object? sender, EventArgs e)
    {
        if (sender is DriveItemViewModel drive)
        {
            await DeleteConnectionAsync(drive);
        }
    }

    private async void OnDriveDuplicateRequested(object? sender, EventArgs e)
    {
        if (sender is DriveItemViewModel drive)
        {
            await DuplicateConnectionAsync(drive);
        }
    }

    private async Task DuplicateConnectionAsync(DriveItemViewModel drive)
    {
        if (drive.IsSftp && drive.SftpConnection is SftpConnection sftpOriginal)
        {
            await DuplicateSftpConnectionAsync(sftpOriginal);
        }
        else if (drive.IsFtp && drive.FtpConnection is FtpConnection ftpOriginal)
        {
            await DuplicateFtpConnectionAsync(ftpOriginal);
        }

        // Refresh UI
        await RefreshDrivesAsync();
    }

    private async Task DuplicateSftpConnectionAsync(SftpConnection original)
    {
        var duplicate = new SftpConnection
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{original.Name} (Copy)",
            Host = original.Host,
            Port = original.Port,
            Username = original.Username,
            AuthType = original.AuthType,
            ObscuredPassword = original.ObscuredPassword,
            KeyFilePath = original.KeyFilePath,
            ObscuredKeyPassphrase = original.ObscuredKeyPassphrase,
            RemotePath = original.RemotePath,
            AutoMount = original.AutoMount,
            MountSettings = new MountSettings
            {
                DriveLetter = null, // Will auto-assign
                NetworkMode = original.MountSettings.NetworkMode,
                VolumeName = original.MountSettings.VolumeName,
                ReadOnly = original.MountSettings.ReadOnly,
                CacheMode = original.MountSettings.CacheMode,
                CacheMaxSize = original.MountSettings.CacheMaxSize,
                DirCacheTimeMinutes = original.MountSettings.DirCacheTimeMinutes
            }
        };

        await _sftpService.CreateRemoteAsync(duplicate);
        await _configManager.AddConnectionAsync(duplicate);
    }

    private async Task DuplicateFtpConnectionAsync(FtpConnection original)
    {
        var duplicate = new FtpConnection
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{original.Name} (Copy)",
            Host = original.Host,
            Port = original.Port,
            Username = original.Username,
            ObscuredPassword = original.ObscuredPassword,
            TlsMode = original.TlsMode,
            PassiveMode = original.PassiveMode,
            RemotePath = original.RemotePath,
            AutoMount = original.AutoMount,
            MountSettings = new MountSettings
            {
                DriveLetter = null, // Will auto-assign
                NetworkMode = original.MountSettings.NetworkMode,
                VolumeName = original.MountSettings.VolumeName,
                ReadOnly = original.MountSettings.ReadOnly,
                CacheMode = original.MountSettings.CacheMode,
                CacheMaxSize = original.MountSettings.CacheMaxSize,
                DirCacheTimeMinutes = original.MountSettings.DirCacheTimeMinutes
            }
        };

        await _ftpService.CreateRemoteAsync(duplicate);
        await _configManager.AddFtpConnectionAsync(duplicate);
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
            if (drive.IsSftp && drive.SftpConnection is SftpConnection sftp)
            {
                await _mountManager.UnmountAsync(sftp.Id);
            }
            else if (drive.IsFtp && drive.FtpConnection is FtpConnection ftp)
            {
                await _mountManager.UnmountAsync(ftp.Id);
            }
        }

        // Delete from rclone config and app config
        if (drive.IsSftp && drive.SftpConnection is SftpConnection sftpConn)
        {
            await _sftpService.DeleteRemoteAsync(sftpConn.RcloneRemoteName);
            await _configManager.DeleteConnectionAsync(sftpConn.Id);
        }
        else if (drive.IsFtp && drive.FtpConnection is FtpConnection ftpConn)
        {
            await _ftpService.DeleteRemoteAsync(ftpConn.RcloneRemoteName);
            await _configManager.DeleteFtpConnectionAsync(ftpConn.Id);
        }

        // Refresh UI
        await RefreshDrivesAsync();
    }

    private void OnMountStatusChanged(object? sender, MountStatusChangedEventArgs e)
    {
        var drive = Drives.FirstOrDefault(d => 
            (d.Connection is SftpConnection sftp && sftp.Id == e.ConnectionId) ||
            (d.Connection is FtpConnection ftp && ftp.Id == e.ConnectionId));
        
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
