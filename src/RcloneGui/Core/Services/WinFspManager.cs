using System.Diagnostics;
using Microsoft.Win32;

namespace RcloneGui.Core.Services;

/// <summary>
/// Manages WinFsp installation detection and installation.
/// </summary>
public class WinFspManager : IWinFspManager
{
    // Check both 64-bit and 32-bit (WOW6432Node) registry paths
    private static readonly string[] WinFspRegistryPaths = [
        @"SOFTWARE\WinFsp",
        @"SOFTWARE\WOW6432Node\WinFsp"
    ];
    private const string WinFspInstallDirValue = "InstallDir";
    private const string WinFspDownloadUrl = "https://github.com/winfsp/winfsp/releases/latest";

    public bool IsInstalled()
    {
        // Check registry for WinFsp installation (both 64-bit and 32-bit paths)
        foreach (var registryPath in WinFspRegistryPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(registryPath);
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
        }

        // Also check common installation paths (both 64-bit and 32-bit Program Files)
        var programFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFiles86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        
        if (Directory.Exists(Path.Combine(programFiles64, "WinFsp")) ||
            Directory.Exists(Path.Combine(programFiles86, "WinFsp")))
        {
            return true;
        }

        // Check if winfsp DLL is available
        var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
        if (File.Exists(Path.Combine(systemPath, "winfsp-x64.dll")) ||
            File.Exists(Path.Combine(systemPath, "winfsp-x86.dll")))
        {
            return true;
        }

        return false;
    }

    public string? GetVersion()
    {
        // Try to get version from registry (both 64-bit and 32-bit paths)
        foreach (var registryPath in WinFspRegistryPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(registryPath);
                if (key != null)
                {
                    var installDir = key.GetValue(WinFspInstallDirValue) as string;
                    if (!string.IsNullOrEmpty(installDir))
                    {
                        // Try both launcher versions
                        var launcher64 = Path.Combine(installDir, "bin", "launcher-x64.exe");
                        var launcher86 = Path.Combine(installDir, "bin", "launcher-x86.exe");
                        
                        var launcherPath = File.Exists(launcher64) ? launcher64 : 
                                          File.Exists(launcher86) ? launcher86 : null;
                        
                        if (launcherPath != null)
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
