namespace RcloneGui.Core.Models;

/// <summary>
/// Represents a mounted drive with its status.
/// </summary>
public class MountedDrive
{
    public string ConnectionId { get; set; } = string.Empty;
    public string? DriveLetter { get; set; }
    public MountStatus Status { get; set; } = MountStatus.Unmounted;
    public string? LastError { get; set; }
    public System.Diagnostics.Process? MountProcess { get; set; }
}

/// <summary>
/// Status of a mounted drive.
/// </summary>
public enum MountStatus
{
    Unmounted,
    Mounting,
    Mounted,
    Unmounting,
    Error
}
