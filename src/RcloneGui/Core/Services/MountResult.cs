using RcloneGui.Core.Models;

namespace RcloneGui.Core.Services;

/// <summary>
/// Result of a mount operation.
/// </summary>
public class MountResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public System.Diagnostics.Process? Process { get; init; }
    public string? DriveLetter { get; init; }
}
