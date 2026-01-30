namespace RcloneGui.Core.Models;

/// <summary>
/// Predefined performance profiles for VFS settings.
/// </summary>
public enum VfsPerformanceProfile
{
    /// <summary>
    /// Default balanced settings.
    /// </summary>
    Default,

    /// <summary>
    /// Optimized for fast streaming of media files.
    /// Large cache, aggressive prefetch, high buffer.
    /// </summary>
    FastStreaming,

    /// <summary>
    /// Optimized for transferring large files.
    /// Large chunks, parallel transfers, full caching.
    /// </summary>
    LargeFiles,

    /// <summary>
    /// Conservative settings for low memory systems.
    /// Minimal caching, small buffers, reduced parallelism.
    /// </summary>
    LowMemory,

    /// <summary>
    /// Optimized for many small files.
    /// High checker count, moderate cache, fast directory scanning.
    /// </summary>
    ManySmallFiles,

    /// <summary>
    /// Maximum performance settings.
    /// Full caching, large buffers, maximum parallelism.
    /// </summary>
    MaximumPerformance,

    /// <summary>
    /// Custom user-defined settings.
    /// </summary>
    Custom
}

/// <summary>
/// Extension methods for VfsPerformanceProfile.
/// </summary>
public static class VfsPerformanceProfileExtensions
{
    /// <summary>
    /// Gets a user-friendly display name for the profile.
    /// </summary>
    public static string GetDisplayName(this VfsPerformanceProfile profile)
    {
        return profile switch
        {
            VfsPerformanceProfile.Default => "Default (Balanced)",
            VfsPerformanceProfile.FastStreaming => "Fast Streaming",
            VfsPerformanceProfile.LargeFiles => "Large Files",
            VfsPerformanceProfile.LowMemory => "Low Memory",
            VfsPerformanceProfile.ManySmallFiles => "Many Small Files",
            VfsPerformanceProfile.MaximumPerformance => "Maximum Performance",
            VfsPerformanceProfile.Custom => "Custom",
            _ => profile.ToString()
        };
    }

    /// <summary>
    /// Gets a description of the profile.
    /// </summary>
    public static string GetDescription(this VfsPerformanceProfile profile)
    {
        return profile switch
        {
            VfsPerformanceProfile.Default => "Balanced settings suitable for most use cases",
            VfsPerformanceProfile.FastStreaming => "Optimized for streaming media with large read-ahead buffers",
            VfsPerformanceProfile.LargeFiles => "Optimized for transferring large files with big chunks",
            VfsPerformanceProfile.LowMemory => "Conservative settings for systems with limited RAM",
            VfsPerformanceProfile.ManySmallFiles => "Optimized for directories with thousands of small files",
            VfsPerformanceProfile.MaximumPerformance => "Maximum speed with full caching and parallelism",
            VfsPerformanceProfile.Custom => "User-defined custom settings",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Applies the performance profile to MountSettings.
    /// </summary>
    public static void ApplyTo(this VfsPerformanceProfile profile, MountSettings settings)
    {
        switch (profile)
        {
            case VfsPerformanceProfile.Default:
                settings.CacheMode = VfsCacheMode.Writes;
                settings.CacheMaxSize = "10G";
                settings.CacheMaxAge = null;
                settings.DirCacheTimeMinutes = 5;
                settings.PollInterval = 60;
                settings.BufferSize = "16M";
                settings.ChunkSize = "64M";
                settings.Transfers = 4;
                settings.Checkers = 8;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.FastStreaming:
                settings.CacheMode = VfsCacheMode.Full;
                settings.CacheMaxSize = "50G";
                settings.CacheMaxAge = "168h";
                settings.DirCacheTimeMinutes = 60;
                settings.PollInterval = 300;
                settings.BufferSize = "64M";
                settings.ChunkSize = "256M";
                settings.Transfers = 8;
                settings.Checkers = 16;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.LargeFiles:
                settings.CacheMode = VfsCacheMode.Full;
                settings.CacheMaxSize = "100G";
                settings.CacheMaxAge = null;
                settings.DirCacheTimeMinutes = 30;
                settings.PollInterval = 120;
                settings.BufferSize = "128M";
                settings.ChunkSize = "512M";
                settings.Transfers = 16;
                settings.Checkers = 32;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.LowMemory:
                settings.CacheMode = VfsCacheMode.Minimal;
                settings.CacheMaxSize = "1G";
                settings.CacheMaxAge = "24h";
                settings.DirCacheTimeMinutes = 2;
                settings.PollInterval = 0;
                settings.BufferSize = "4M";
                settings.ChunkSize = "16M";
                settings.Transfers = 2;
                settings.Checkers = 4;
                settings.AsyncRead = false;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.ManySmallFiles:
                settings.CacheMode = VfsCacheMode.Writes;
                settings.CacheMaxSize = "5G";
                settings.CacheMaxAge = "72h";
                settings.DirCacheTimeMinutes = 60;
                settings.PollInterval = 60;
                settings.BufferSize = "8M";
                settings.ChunkSize = "32M";
                settings.Transfers = 8;
                settings.Checkers = 64;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.MaximumPerformance:
                settings.CacheMode = VfsCacheMode.Full;
                settings.CacheMaxSize = "200G";
                settings.CacheMaxAge = null;
                settings.DirCacheTimeMinutes = 120;
                settings.PollInterval = 300;
                settings.BufferSize = "256M";
                settings.ChunkSize = "1G";
                settings.Transfers = 32;
                settings.Checkers = 64;
                settings.AsyncRead = true;
                settings.AsyncWrite = true;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.Custom:
                // Don't modify settings - user maintains custom values
                break;
        }

        settings.PerformanceProfile = profile;
    }

    /// <summary>
    /// Applies the performance profile to GlobalVfsSettings.
    /// </summary>
    public static void ApplyTo(this VfsPerformanceProfile profile, GlobalVfsSettings settings)
    {
        switch (profile)
        {
            case VfsPerformanceProfile.Default:
                settings.CacheMode = VfsCacheMode.Writes;
                settings.CacheMaxSize = "10G";
                settings.CacheMaxAge = null;
                settings.DirCacheTimeMinutes = 5;
                settings.PollInterval = 60;
                settings.BufferSize = "16M";
                settings.ChunkSize = "64M";
                settings.Transfers = 4;
                settings.Checkers = 8;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.FastStreaming:
                settings.CacheMode = VfsCacheMode.Full;
                settings.CacheMaxSize = "50G";
                settings.CacheMaxAge = "168h";
                settings.DirCacheTimeMinutes = 60;
                settings.PollInterval = 300;
                settings.BufferSize = "64M";
                settings.ChunkSize = "256M";
                settings.Transfers = 8;
                settings.Checkers = 16;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.LargeFiles:
                settings.CacheMode = VfsCacheMode.Full;
                settings.CacheMaxSize = "100G";
                settings.CacheMaxAge = null;
                settings.DirCacheTimeMinutes = 30;
                settings.PollInterval = 120;
                settings.BufferSize = "128M";
                settings.ChunkSize = "512M";
                settings.Transfers = 16;
                settings.Checkers = 32;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.LowMemory:
                settings.CacheMode = VfsCacheMode.Minimal;
                settings.CacheMaxSize = "1G";
                settings.CacheMaxAge = "24h";
                settings.DirCacheTimeMinutes = 2;
                settings.PollInterval = 0;
                settings.BufferSize = "4M";
                settings.ChunkSize = "16M";
                settings.Transfers = 2;
                settings.Checkers = 4;
                settings.AsyncRead = false;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.ManySmallFiles:
                settings.CacheMode = VfsCacheMode.Writes;
                settings.CacheMaxSize = "5G";
                settings.CacheMaxAge = "72h";
                settings.DirCacheTimeMinutes = 60;
                settings.PollInterval = 60;
                settings.BufferSize = "8M";
                settings.ChunkSize = "32M";
                settings.Transfers = 8;
                settings.Checkers = 64;
                settings.AsyncRead = true;
                settings.AsyncWrite = false;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.MaximumPerformance:
                settings.CacheMode = VfsCacheMode.Full;
                settings.CacheMaxSize = "200G";
                settings.CacheMaxAge = null;
                settings.DirCacheTimeMinutes = 120;
                settings.PollInterval = 300;
                settings.BufferSize = "256M";
                settings.ChunkSize = "1G";
                settings.Transfers = 32;
                settings.Checkers = 64;
                settings.AsyncRead = true;
                settings.AsyncWrite = true;
                settings.Umask = "000";
                settings.UID = 0;
                settings.GID = 0;
                break;

            case VfsPerformanceProfile.Custom:
                // Don't modify settings - user maintains custom values
                break;
        }

        settings.PerformanceProfile = profile;
        settings.IsCustomized = profile != VfsPerformanceProfile.Default;
    }
}
