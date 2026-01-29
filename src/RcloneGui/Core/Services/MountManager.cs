using System.Collections.Concurrent;
using RcloneGui.Core.Models;
using RcloneGui.Features.Sftp.Models;
using RcloneGui.Features.Ftp.Models;

namespace RcloneGui.Core.Services;

/// <summary>
/// Manages active mounts and mount operations.
/// </summary>
public class MountManager : IMountManager
{
    private readonly ISftpService _sftpService;
    private readonly IFtpService _ftpService;
    private readonly IConfigManager _configManager;
    private readonly INotificationService _notificationService;
    private readonly ConcurrentDictionary<string, MountedDrive> _mounts = new();

    public IReadOnlyList<MountedDrive> MountedDrives => _mounts.Values.ToList().AsReadOnly();

    public event EventHandler<MountStatusChangedEventArgs>? MountStatusChanged;

    public MountManager(ISftpService sftpService, IFtpService ftpService, IConfigManager configManager, INotificationService notificationService)
    {
        _sftpService = sftpService;
        _ftpService = ftpService;
        _configManager = configManager;
        _notificationService = notificationService;
    }

    public async Task<MountedDrive> MountAsync(SftpConnection connection, string? preferredDriveLetter = null)
    {
        return await MountInternalAsync(connection, preferredDriveLetter, async (c, d) => await _sftpService.MountAsync(c, d));
    }

    public async Task<MountedDrive> MountAsync(FtpConnection connection, string? preferredDriveLetter = null)
    {
        return await MountInternalAsync(connection, preferredDriveLetter, async (c, d) => await _ftpService.MountAsync(c, d));
    }

    private async Task<MountedDrive> MountInternalAsync<T>(T connection, string? preferredDriveLetter, Func<T, string, Task<MountResult>> mountFunc) where T : class
    {
        // Determine drive letter
        var driveLetter = preferredDriveLetter ?? GetMountSettings(connection)?.DriveLetter;
        
        if (string.IsNullOrEmpty(driveLetter))
        {
            var available = _sftpService.GetAvailableDriveLetters();
            driveLetter = available.FirstOrDefault() ?? throw new InvalidOperationException("No available drive letters");
        }

        var connectionId = GetConnectionId(connection);
        var connectionName = GetConnectionName(connection);

        var mountedDrive = new MountedDrive
        {
            ConnectionId = connectionId,
            DriveLetter = driveLetter,
            Status = MountStatus.Mounting
        };

        _mounts[connectionId] = mountedDrive;
        RaiseMountStatusChanged(connectionId, MountStatus.Unmounted, MountStatus.Mounting, driveLetter);

        try
        {
            var result = await mountFunc(connection, driveLetter);

            if (result.Success)
            {
                mountedDrive.Status = MountStatus.Mounted;
                mountedDrive.MountProcess = result.Process;
                
                // Monitor process for unexpected exit
                if (result.Process != null)
                {
                    _ = MonitorProcessAsync(connectionId, result.Process);
                }

                RaiseMountStatusChanged(connectionId, MountStatus.Mounting, MountStatus.Mounted, driveLetter);
                
                // Show notification
                _notificationService.ShowMountNotification(connectionName, driveLetter);
            }
            else
            {
                mountedDrive.Status = MountStatus.Error;
                mountedDrive.LastError = result.ErrorMessage;
                RaiseMountStatusChanged(connectionId, MountStatus.Mounting, MountStatus.Error, driveLetter, result.ErrorMessage);
                
                // Show error notification
                _notificationService.ShowMountErrorNotification(connectionName, result.ErrorMessage ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            mountedDrive.Status = MountStatus.Error;
            mountedDrive.LastError = ex.Message;
            RaiseMountStatusChanged(connectionId, MountStatus.Mounting, MountStatus.Error, driveLetter, ex.Message);
            
            // Show error notification
            _notificationService.ShowMountErrorNotification(connectionName, ex.Message);
        }

        return mountedDrive;
    }

    private static string GetConnectionId(object connection)
    {
        return connection switch
        {
            SftpConnection sftp => sftp.Id,
            FtpConnection ftp => ftp.Id,
            _ => throw new ArgumentException("Unknown connection type")
        };
    }

    private static string GetConnectionName(object connection)
    {
        return connection switch
        {
            SftpConnection sftp => sftp.Name,
            FtpConnection ftp => ftp.Name,
            _ => throw new ArgumentException("Unknown connection type")
        };
    }

    private static MountSettings? GetMountSettings(object connection)
    {
        return connection switch
        {
            SftpConnection sftp => sftp.MountSettings,
            FtpConnection ftp => ftp.MountSettings,
            _ => null
        };
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
            if (mountedDrive.MountProcess != null && !mountedDrive.MountProcess.HasExited)
            {
                mountedDrive.MountProcess.Kill();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                try
                {
                    await mountedDrive.MountProcess.WaitForExitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Process didn't exit in time, continue anyway
                }
            }
            else
            {
                // Try to unmount via service
                if (mountedDrive.DriveLetter != null)
                {
                    await _sftpService.UnmountAsync(mountedDrive.DriveLetter);
                }
            }

            mountedDrive.Status = MountStatus.Unmounted;
            mountedDrive.MountProcess = null;
            
            var driveLetter = mountedDrive.DriveLetter;
            
            _mounts.TryRemove(connectionId, out _);
            
            RaiseMountStatusChanged(connectionId, MountStatus.Unmounting, MountStatus.Unmounted, driveLetter);
            
            // Show notification
            _notificationService.ShowUnmountNotification(connectionId, driveLetter ?? "?");
            
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

        // Auto-mount SFTP connections
        var sftpConnections = _configManager.GetConnections()
            .Where(c => c.AutoMount)
            .ToList();

        foreach (var connection in sftpConnections)
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

        // Auto-mount FTP connections
        var ftpConnections = _configManager.GetFtpConnections()
            .Where(c => c.AutoMount)
            .ToList();

        foreach (var connection in ftpConnections)
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
