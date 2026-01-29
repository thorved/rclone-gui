using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Models;
using RcloneGui.Services;

namespace RcloneGui.ViewModels;

/// <summary>
/// ViewModel for a drive item in the list.
/// </summary>
public partial class DriveItemViewModel : ObservableObject
{
    private readonly IMountManager _mountManager;
    private readonly IRcloneService _rcloneService;

    public SftpConnection Connection { get; }

    [ObservableProperty]
    private MountStatus _status = MountStatus.Unmounted;

    [ObservableProperty]
    private string? _driveLetter;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public string Name => Connection.Name;
    public string Host => $"{Connection.Host}:{Connection.Port}";
    public string RemotePath => Connection.RemotePath;
    public bool AutoMount => Connection.AutoMount;

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

    public DriveItemViewModel(SftpConnection connection, IMountManager mountManager, IRcloneService rcloneService)
    {
        Connection = connection;
        _mountManager = mountManager;
        _rcloneService = rcloneService;
        _driveLetter = connection.MountSettings.DriveLetter;
    }

    [RelayCommand]
    private async Task MountAsync()
    {
        if (Status == MountStatus.Mounted || IsBusy) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _mountManager.MountAsync(Connection, DriveLetter);
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
            var success = await _mountManager.UnmountAsync(Connection.Id);
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
            var (success, message) = await _rcloneService.TestConnectionAsync(Connection);
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
    }
}
