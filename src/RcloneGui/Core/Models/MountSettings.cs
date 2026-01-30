namespace RcloneGui.Core.Models;

/// <summary>
/// Mount-specific settings for a connection.
/// </summary>
public class MountSettings
{
    /// <summary>
    /// Preferred drive letter (A-Z) or empty for auto-assign.
    /// </summary>
    public string DriveLetter { get; set; } = string.Empty;

    /// <summary>
    /// Whether to mount as network drive instead of fixed disk.
    /// </summary>
    public bool NetworkMode { get; set; } = true;

    /// <summary>
    /// Custom volume name displayed in Explorer.
    /// </summary>
    public string? VolumeName { get; set; }

    /// <summary>
    /// Whether to mount as read-only.
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// VFS cache mode: off, minimal, writes, full.
    /// </summary>
    public VfsCacheMode CacheMode { get; set; } = VfsCacheMode.Writes;

    /// <summary>
    /// Maximum VFS cache size (e.g., "10G", "500M").
    /// </summary>
    public string CacheMaxSize { get; set; } = "10G";

    /// <summary>
    /// Directory cache time in minutes.
    /// </summary>
    public int DirCacheTimeMinutes { get; set; } = 5;

    // ==================== ADVANCED VFS SETTINGS ====================

    /// <summary>
    /// Maximum age of cached files (e.g., "72h", "7d"). Empty = no limit.
    /// </summary>
    public string? CacheMaxAge { get; set; } = null;

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
    /// Whether to use default global settings for this connection.
    /// When true, all VFS settings above are ignored and global defaults are used.
    /// </summary>
    public bool UseGlobalVfsSettings { get; set; } = true;

    /// <summary>
    /// Apply a performance profile to quickly set recommended values.
    /// </summary>
    public VfsPerformanceProfile PerformanceProfile { get; set; } = VfsPerformanceProfile.Default;

    /// <summary>
    /// Creates a deep copy of these settings.
    /// </summary>
    public MountSettings Clone()
    {
        return new MountSettings
        {
            DriveLetter = DriveLetter,
            NetworkMode = NetworkMode,
            VolumeName = VolumeName,
            ReadOnly = ReadOnly,
            CacheMode = CacheMode,
            CacheMaxSize = CacheMaxSize,
            DirCacheTimeMinutes = DirCacheTimeMinutes,
            CacheMaxAge = CacheMaxAge,
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
            UseGlobalVfsSettings = UseGlobalVfsSettings,
            PerformanceProfile = PerformanceProfile
        };
    }
}
