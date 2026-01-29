using System.Diagnostics;
using Microsoft.Win32;

namespace RcloneGui.Services;

/// <summary>
/// Manages WinFsp installation detection and installation.
/// </summary>
public class WinFspManager : IWinFspManager
{
    private const string WinFspRegistryPath = @"SOFTWARE\WinFsp";
    private const string WinFspInstallDirValue = "InstallDir";
    private const string WinFspDownloadUrl = "https://github.com/winfsp/winfsp/releases/latest";

    public bool IsInstalled()
    {
        // Check registry for WinFsp installation
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinFspRegistryPath);
            if (key != null)
            {
                var installDir = key.GetValue(WinFspInstallDirValue) as string;
                if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
                {
                    return true;
                }
            }
        }
        catch
        {
            // Registry access may fail
        }

        // Also check common installation paths
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var winfspPath = Path.Combine(programFiles, "WinFsp");
        if (Directory.Exists(winfspPath))
        {
            return true;
        }

        // Check if winfsp DLL is available
        var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var winfspDll = Path.Combine(systemPath, "winfsp-x64.dll");
        if (File.Exists(winfspDll))
        {
            return true;
        }

        return false;
    }

    public string? GetVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinFspRegistryPath);
            if (key != null)
            {
                var installDir = key.GetValue(WinFspInstallDirValue) as string;
                if (!string.IsNullOrEmpty(installDir))
                {
                    var launcherPath = Path.Combine(installDir, "bin", "launcher-x64.exe");
                    if (File.Exists(launcherPath))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(launcherPath);
                        return versionInfo.FileVersion;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    public async Task<(bool Success, string Message)> InstallAsync()
    {
        // Check if bundled installer exists
        var appDir = AppContext.BaseDirectory;
        var installerPath = Path.Combine(appDir, "Assets", "winfsp.msi");

        if (!File.Exists(installerPath))
        {
            // Download URL for manual install
            return (false, $"WinFsp installer not found. Please download and install from: {WinFspDownloadUrl}");
        }

        try
        {
            // Run MSI installer silently
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = $"/i \"{installerPath}\" /qn /norestart",
                    UseShellExecute = true,
                    Verb = "runas" // Request elevation
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                return (true, "WinFsp installed successfully. Please restart the application.");
            }
            else if (process.ExitCode == 1602)
            {
                return (false, "Installation was cancelled by user.");
            }
            else
            {
                return (false, $"Installation failed with exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Installation failed: {ex.Message}");
        }
    }

    public string GetDownloadUrl()
    {
        return WinFspDownloadUrl;
    }
}
