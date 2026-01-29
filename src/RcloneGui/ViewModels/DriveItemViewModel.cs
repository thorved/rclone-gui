using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Models;
using RcloneGui.Services;

namespace RcloneGui.ViewModels;

/// <summary>
/// ViewModel for a drive item in the list (supports both SFTP and FTP).
/// </summary>
public partial class DriveItemViewModel : ObservableObject
{
    private readonly IMountManager _mountManager;
    private readonly IRcloneService _rcloneService;

    /// <summary>
    /// The connection (SftpConnection or FtpConnection).
    /// </summary>
    public object Connection { get; }

    [ObservableProperty]
    private MountStatus _status = MountStatus.Unmounted;

    [ObservableProperty]
    private string? _driveLetter;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    // Connection type helpers
    public bool IsSftp => Connection is SftpConnection;
    public bool IsFtp => Connection is FtpConnection;

    // SFTP connection (null if FTP)
    public SftpConnection? SftpConnection => Connection as SftpConnection;
    
    // FTP connection (null if SFTP)
    public FtpConnection? FtpConnection => Connection as FtpConnection;

    public string Name => Connection switch
    {
        SftpConnection sftp => sftp.Name,
        FtpConnection ftp => ftp.Name,
        _ => "Unknown"
    };

    public string Host => Connection switch
    {
        SftpConnection sftp => $"{sftp.Host}:{sftp.Port}",
        FtpConnection ftp => $"{ftp.Host}:{ftp.Port}",
        _ => "Unknown"
    };

    public string RemotePath => Connection switch
    {
        SftpConnection sftp => sftp.RemotePath,
        FtpConnection ftp => ftp.RemotePath,
        _ => "/"
    };

    public bool AutoMount => Connection switch
    {
        SftpConnection sftp => sftp.AutoMount,
        FtpConnection ftp => ftp.AutoMount,
        _ => false
    };

    public string ConnectionType => Connection switch
    {
        SftpConnection _ => "SFTP",
        FtpConnection _ => "FTP",
        _ => "Unknown"
    };

    public string StatusText => Status switch
    {
        MountStatus.Unmounted => "Disconnected",
        MountStatus.Mounting => "Connecting...",
        MountStatus.Mounted => $"Connected ({DriveLetter}:)",
        MountStatus.Unmounting => "Disconnecting...",
        MountStatus.Error => "Error",
        _ => "Unknown"
    };

    public string StatusIcon => Status switch
    {
        MountStatus.Unmounted => "\uE8D8", // Disconnected icon
        MountStatus.Mounting => "\uE895", // Sync icon
        MountStatus.Mounted => "\uE8CE", // Connected icon  
        MountStatus.Unmounting => "\uE895", // Sync icon
        MountStatus.Error => "\uEA39", // Error icon
        _ => "\uE9CE"
    };

    /// <summary>
    /// Returns true if the drive can be mounted (not already mounted or busy).
    /// </summary>
    public bool CanMount => Status == MountStatus.Unmounted || Status == MountStatus.Error;

    /// <summary>
    /// Returns true if the drive can be unmounted (currently mounted).
    /// </summary>
    public bool CanUnmount => Status == MountStatus.Mounted;

    /// <summary>
    /// Returns true if the drive is currently mounted.
    /// </summary>
    public bool IsMounted => Status == MountStatus.Mounted;

    /// <summary>
    /// Icon for toggle mount button
    /// </summary>
    public string ToggleMountIcon => IsMounted ? "\uE74B" : "\uE768"; // Eject vs Mount

    /// <summary>
    /// Tooltip for toggle mount button
    /// </summary>
    public string ToggleMountTooltip => IsMounted ? "Unmount Drive" : "Mount Drive";

    /// <summary>
    /// Drive size text (if mounted)
    /// </summary>
    public string SizeText
    {
        get
        {
            if (!IsMounted || string.IsNullOrEmpty(DriveLetter)) return "";
            
            try
            {
                var driveInfo = new DriveInfo(DriveLetter);
                if (driveInfo.IsReady)
                {
                    var totalGB = driveInfo.TotalSize / (1024.0 * 1024 * 1024);
                    var freeGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                    return $"{freeGB:F1} GB free of {totalGB:F1} GB";
                }
            }
            catch { }
            
            return "";
        }
    }

    public DriveItemViewModel(object connection, IMountManager mountManager, IRcloneService rcloneService)
    {
        if (!(connection is SftpConnection) && !(connection is FtpConnection))
        {
            throw new ArgumentException("Connection must be either SftpConnection or FtpConnection", nameof(connection));
        }
        
        Connection = connection;
        _mountManager = mountManager;
        _rcloneService = rcloneService;
        _driveLetter = GetMountSettings()?.DriveLetter;
    }

    private MountSettings? GetMountSettings()
    {
        return Connection switch
        {
            SftpConnection sftp => sftp.MountSettings,
            FtpConnection ftp => ftp.MountSettings,
            _ => null
        };
    }

    private string GetConnectionId()
    {
        return Connection switch
        {
            SftpConnection sftp => sftp.Id,
            FtpConnection ftp => ftp.Id,
            _ => throw new InvalidOperationException("Unknown connection type")
        };
    }

    [RelayCommand]
    private async Task MountAsync()
    {
        if (Status == MountStatus.Mounted || IsBusy) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            MountedDrive result;
            if (Connection is SftpConnection sftp)
            {
                result = await _mountManager.MountAsync(sftp, DriveLetter);
            }
            else if (Connection is FtpConnection ftp)
            {
                result = await _mountManager.MountAsync(ftp, DriveLetter);
            }
            else
            {
                throw new InvalidOperationException("Unknown connection type");
            }
            
            DriveLetter = result.DriveLetter;
            Status = result.Status;
            
            if (result.Status == MountStatus.Error)
            {
                ErrorMessage = result.LastError;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Status = MountStatus.Error;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UnmountAsync()
    {
        if (Status != MountStatus.Mounted || IsBusy) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var connectionId = GetConnectionId();
            var success = await _mountManager.UnmountAsync(connectionId);
            if (success)
            {
                Status = MountStatus.Unmounted;
            }
            else
            {
                ErrorMessage = "Failed to unmount drive";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleMountAsync()
    {
        if (Status == MountStatus.Mounted)
        {
            await UnmountAsync();
        }
        else if (Status == MountStatus.Unmounted || Status == MountStatus.Error)
        {
            await MountAsync();
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            bool success;
            string message;
            
            if (Connection is SftpConnection sftp)
            {
                (success, message) = await _rcloneService.TestConnectionAsync(sftp);
            }
            else if (Connection is FtpConnection ftp)
            {
                (success, message) = await _rcloneService.TestFtpConnectionAsync(ftp);
            }
            else
            {
                throw new InvalidOperationException("Unknown connection type");
            }
            
            if (!success)
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenDrive()
    {
        if (Status == MountStatus.Mounted && !string.IsNullOrEmpty(DriveLetter))
        {
            var path = $"{DriveLetter}:\\";
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }

    /// <summary>
    /// Alias for OpenDrive command for XAML binding as OpenCommand.
    /// </summary>
    public IRelayCommand OpenCommand => OpenDriveCommand;

    /// <summary>
    /// Event triggered when the user wants to edit this connection.
    /// </summary>
    public event EventHandler? EditRequested;

    /// <summary>
    /// Event triggered when the user wants to delete this connection.
    /// </summary>
    public event EventHandler? DeleteRequested;

    /// <summary>
    /// Event triggered when the user wants to duplicate this connection.
    /// </summary>
    public event EventHandler? DuplicateRequested;

    [RelayCommand]
    private void Edit()
    {
        EditRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Delete()
    {
        DeleteRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Duplicate()
    {
        DuplicateRequested?.Invoke(this, EventArgs.Empty);
    }

    partial void OnStatusChanged(MountStatus value)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(CanMount));
        OnPropertyChanged(nameof(CanUnmount));
        OnPropertyChanged(nameof(IsMounted));
        OnPropertyChanged(nameof(ToggleMountIcon));
        OnPropertyChanged(nameof(ToggleMountTooltip));
        OnPropertyChanged(nameof(SizeText));
    }
}
