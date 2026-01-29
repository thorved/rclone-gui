namespace RcloneGui.Core.Services;

/// <summary>
/// Interface for WinFsp management.
/// </summary>
public interface IWinFspManager
{
    /// <summary>
    /// Checks if WinFsp is installed.
    /// </summary>
    bool IsInstalled();

    /// <summary>
    /// Gets the installed WinFsp version.
    /// </summary>
    string? GetVersion();

    /// <summary>
    /// Installs WinFsp from bundled installer.
    /// </summary>
    Task<(bool Success, string Message)> InstallAsync();

    /// <summary>
    /// Gets the WinFsp download URL.
    /// </summary>
    string GetDownloadUrl();
}
