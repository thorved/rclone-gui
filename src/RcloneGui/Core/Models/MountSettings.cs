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
}
