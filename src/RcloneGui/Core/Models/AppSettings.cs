using RcloneGui.Features.Sftp.Models;
using RcloneGui.Features.Ftp.Models;

namespace RcloneGui.Core.Models;

/// <summary>
/// Application-wide settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Whether to start the application minimized to system tray.
    /// </summary>
    public bool StartMinimized { get; set; } = false;

    /// <summary>
    /// Whether to minimize to tray instead of closing.
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Whether to start with Windows.
    /// </summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Whether to auto-mount connections on startup.
    /// </summary>
    public bool AutoMountOnStartup { get; set; } = true;

    /// <summary>
    /// Whether to unmount all drives when application exits.
    /// </summary>
    public bool UnmountOnClose { get; set; } = true;

    /// <summary>
    /// Whether to show notifications for mount events.
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// Theme setting: System, Light, or Dark.
    /// </summary>
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>
    /// Path to custom rclone executable (empty = use bundled).
    /// </summary>
    public string? CustomRclonePath { get; set; }

    /// <summary>
    /// Default VFS cache location.
    /// </summary>
    public string? CacheDirectory { get; set; }

    /// <summary>
    /// List of configured SFTP connections.
    /// </summary>
    public List<SftpConnection> Connections { get; set; } = new();

    /// <summary>
    /// List of configured FTP connections.
    /// </summary>
    public List<FtpConnection> FtpConnections { get; set; } = new();

    /// <summary>
    /// Global VFS and performance settings applied to all mounts by default.
    /// </summary>
    public GlobalVfsSettings GlobalVfsSettings { get; set; } = new();
}
