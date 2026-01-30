namespace RcloneGui.Core.Models;

/// <summary>
/// Global VFS and performance settings for rclone.
/// These are the default settings applied to all mounts unless overridden per-drive.
/// </summary>
public class GlobalVfsSettings
{
    /// <summary>
    /// VFS cache mode: off, minimal, writes, full.
    /// </summary>
    public VfsCacheMode CacheMode { get; set; } = VfsCacheMode.Writes;

    /// <summary>
    /// Maximum VFS cache size (e.g., "10G", "500M").
    /// </summary>
    public string CacheMaxSize { get; set; } = "10G";

    /// <summary>
    /// Maximum age of cached files (e.g., "72h", "7d"). Empty = no limit.
    /// </summary>
    public string? CacheMaxAge { get; set; } = null;

    /// <summary>
    /// Directory cache time in minutes.
    /// </summary>
    public int DirCacheTimeMinutes { get; set; } = 5;

    /// <summary>
    /// Poll interval in seconds to check for remote changes. 0 = disabled.
    /// </summary>
    public int PollInterval { get; set; } = 60;

    /// <summary>
    /// Memory buffer size for transfers (e.g., "16M", "32M").
    /// </summary>
    public string BufferSize { get; set; } = "16M";

    /// <summary>
    /// Chunk size for transfers (e.g., "64M", "128M").
    /// </summary>
    public string ChunkSize { get; set; } = "64M";

    /// <summary>
    /// Number of parallel transfers.
    /// </summary>
    public int Transfers { get; set; } = 4;

    /// <summary>
    /// Number of parallel checkers for directory scanning.
    /// </summary>
    public int Checkers { get; set; } = 8;

    /// <summary>
    /// Enable asynchronous read operations.
    /// </summary>
    public bool AsyncRead { get; set; } = true;

    /// <summary>
    /// Enable asynchronous write operations.
    /// </summary>
    public bool AsyncWrite { get; set; } = false;

    /// <summary>
    /// File permission mask (umask) in octal (e.g., "000", "022").
    /// </summary>
    public string Umask { get; set; } = "000";

    /// <summary>
    /// User ID for file ownership. 0 = default.
    /// </summary>
    public int UID { get; set; } = 0;

    /// <summary>
    /// Group ID for file ownership. 0 = default.
    /// </summary>
    public int GID { get; set; } = 0;

    /// <summary>
    /// Currently selected performance profile.
    /// </summary>
    public VfsPerformanceProfile PerformanceProfile { get; set; } = VfsPerformanceProfile.Default;

    /// <summary>
    /// Whether these settings have been customized from defaults.
    /// Used to show indicator in UI.
    /// </summary>
    public bool IsCustomized { get; set; } = false;

    /// <summary>
    /// Applies a performance profile to these settings.
    /// </summary>
    public void ApplyProfile(VfsPerformanceProfile profile)
    {
        profile.ApplyTo(this);
        PerformanceProfile = profile;
        IsCustomized = true;
    }

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        CacheMode = VfsCacheMode.Writes;
        CacheMaxSize = "10G";
        CacheMaxAge = null;
        DirCacheTimeMinutes = 5;
        PollInterval = 60;
        BufferSize = "16M";
        ChunkSize = "64M";
        Transfers = 4;
        Checkers = 8;
        AsyncRead = true;
        AsyncWrite = false;
        Umask = "000";
        UID = 0;
        GID = 0;
        PerformanceProfile = VfsPerformanceProfile.Default;
        IsCustomized = false;
    }

    /// <summary>
    /// Creates a MountSettings instance from these global settings.
    /// Used when applying global defaults to a connection.
    /// </summary>
    public MountSettings ToMountSettings()
    {
        return new MountSettings
        {
            CacheMode = CacheMode,
            CacheMaxSize = CacheMaxSize,
            CacheMaxAge = CacheMaxAge,
            DirCacheTimeMinutes = DirCacheTimeMinutes,
            PollInterval = PollInterval,
            BufferSize = BufferSize,
            ChunkSize = ChunkSize,
            Transfers = Transfers,
            Checkers = Checkers,
            AsyncRead = AsyncRead,
            AsyncWrite = AsyncWrite,
            Umask = Umask,
            UID = UID,
            GID = GID,
            PerformanceProfile = PerformanceProfile,
            UseGlobalVfsSettings = true
        };
    }

    /// <summary>
    /// Creates a deep copy of these settings.
    /// </summary>
    public GlobalVfsSettings Clone()
    {
        return new GlobalVfsSettings
        {
            CacheMode = CacheMode,
            CacheMaxSize = CacheMaxSize,
            CacheMaxAge = CacheMaxAge,
            DirCacheTimeMinutes = DirCacheTimeMinutes,
            PollInterval = PollInterval,
            BufferSize = BufferSize,
            ChunkSize = ChunkSize,
            Transfers = Transfers,
            Checkers = Checkers,
            AsyncRead = AsyncRead,
            AsyncWrite = AsyncWrite,
            Umask = Umask,
            UID = UID,
            GID = GID,
            PerformanceProfile = PerformanceProfile,
            IsCustomized = IsCustomized
        };
    }
}
