using System.Collections.Concurrent;
using RcloneGui.Models;

namespace RcloneGui.Services;

/// <summary>
/// Manages active mounts and mount operations.
/// </summary>
public class MountManager : IMountManager
{
    private readonly IRcloneService _rcloneService;
    private readonly IConfigManager _configManager;
    private readonly ConcurrentDictionary<string, MountedDrive> _mounts = new();

    public IReadOnlyList<MountedDrive> MountedDrives => _mounts.Values.ToList().AsReadOnly();

    public event EventHandler<MountStatusChangedEventArgs>? MountStatusChanged;

    public MountManager(IRcloneService rcloneService, IConfigManager configManager)
    {
        _rcloneService = rcloneService;
        _configManager = configManager;
    }

    public async Task<MountedDrive> MountAsync(SftpConnection connection, string? preferredDriveLetter = null)
    {
        // Determine drive letter
        var driveLetter = preferredDriveLetter ?? connection.MountSettings.DriveLetter;
        
        if (string.IsNullOrEmpty(driveLetter))
        {
            var available = _rcloneService.GetAvailableDriveLetters();
            driveLetter = available.FirstOrDefault() ?? throw new InvalidOperationException("No available drive letters");
        }

        var mountedDrive = new MountedDrive
        {
            Connection = connection,
            DriveLetter = driveLetter,
            Status = MountStatus.Mounting
        };

        _mounts[connection.Id] = mountedDrive;
        RaiseMountStatusChanged(connection.Id, MountStatus.Unmounted, MountStatus.Mounting, driveLetter);

        try
        {
            var result = await _rcloneService.MountAsync(connection, driveLetter);

            if (result.Success)
            {
                mountedDrive.Status = MountStatus.Mounted;
                mountedDrive.RcloneProcess = result.Process;
                mountedDrive.MountedAt = DateTime.UtcNow;
                
                // Monitor process for unexpected exit
                if (result.Process != null)
                {
                    _ = MonitorProcessAsync(connection.Id, result.Process);
                }

                RaiseMountStatusChanged(connection.Id, MountStatus.Mounting, MountStatus.Mounted, driveLetter);
            }
            else
            {
                mountedDrive.Status = MountStatus.Error;
                mountedDrive.LastError = result.ErrorMessage;
                RaiseMountStatusChanged(connection.Id, MountStatus.Mounting, MountStatus.Error, driveLetter, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            mountedDrive.Status = MountStatus.Error;
            mountedDrive.LastError = ex.Message;
            RaiseMountStatusChanged(connection.Id, MountStatus.Mounting, MountStatus.Error, driveLetter, ex.Message);
        }

        return mountedDrive;
    }

    public async Task<bool> UnmountAsync(string connectionId)
    {
        if (!_mounts.TryGetValue(connectionId, out var mountedDrive))
        {
            return false;
        }

        var oldStatus = mountedDrive.Status;
        mountedDrive.Status = MountStatus.Unmounting;
        RaiseMountStatusChanged(connectionId, oldStatus, MountStatus.Unmounting, mountedDrive.DriveLetter);

        try
        {
            // Kill the rclone process if running
            if (mountedDrive.RcloneProcess != null && !mountedDrive.RcloneProcess.HasExited)
            {
                mountedDrive.RcloneProcess.Kill();
                await mountedDrive.RcloneProcess.WaitForExitAsync();
            }
            else
            {
                // Try to unmount via rclone service
                await _rcloneService.UnmountAsync(mountedDrive.DriveLetter);
            }

            mountedDrive.Status = MountStatus.Unmounted;
            mountedDrive.RcloneProcess = null;
            mountedDrive.MountedAt = null;
            
            _mounts.TryRemove(connectionId, out _);
            
            RaiseMountStatusChanged(connectionId, MountStatus.Unmounting, MountStatus.Unmounted, mountedDrive.DriveLetter);
            return true;
        }
        catch (Exception ex)
        {
            mountedDrive.Status = MountStatus.Error;
            mountedDrive.LastError = ex.Message;
            RaiseMountStatusChanged(connectionId, MountStatus.Unmounting, MountStatus.Error, mountedDrive.DriveLetter, ex.Message);
            return false;
        }
    }

    public async Task UnmountAllAsync()
    {
        var connectionIds = _mounts.Keys.ToList();
        foreach (var connectionId in connectionIds)
        {
            await UnmountAsync(connectionId);
        }
    }

    public async Task AutoMountAsync()
    {
        if (_configManager.Settings?.AutoMountOnStartup != true)
        {
            return;
        }

        var connections = _configManager.GetConnections()
            .Where(c => c.AutoMount)
            .ToList();

        foreach (var connection in connections)
        {
            try
            {
                await MountAsync(connection);
            }
            catch
            {
                // Log but continue with other mounts
            }
        }
    }

    public MountedDrive? GetMountStatus(string connectionId)
    {
        _mounts.TryGetValue(connectionId, out var mount);
        return mount;
    }

    public bool IsMounted(string connectionId)
    {
        return _mounts.TryGetValue(connectionId, out var mount) && mount.Status == MountStatus.Mounted;
    }

    private async Task MonitorProcessAsync(string connectionId, System.Diagnostics.Process process)
    {
        try
        {
            await process.WaitForExitAsync();
            
            if (_mounts.TryGetValue(connectionId, out var mount))
            {
                if (mount.Status == MountStatus.Mounted)
                {
                    // Unexpected exit
                    mount.Status = MountStatus.Error;
                    mount.LastError = "Mount process terminated unexpectedly";
                    RaiseMountStatusChanged(connectionId, MountStatus.Mounted, MountStatus.Error, mount.DriveLetter, mount.LastError);
                }
            }
        }
        catch
        {
            // Process monitoring failed
        }
    }

    private void RaiseMountStatusChanged(string connectionId, MountStatus oldStatus, MountStatus newStatus, string? driveLetter, string? errorMessage = null)
    {
        MountStatusChanged?.Invoke(this, new MountStatusChangedEventArgs
        {
            ConnectionId = connectionId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            DriveLetter = driveLetter,
            ErrorMessage = errorMessage
        });
    }
}
