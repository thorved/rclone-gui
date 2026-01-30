using System.Diagnostics;
using System.Text;
using RcloneGui.Core.Models;
using RcloneGui.Features.Sftp.Models;

namespace RcloneGui.Core.Services;

/// <summary>
/// Service for executing rclone commands for SFTP.
/// </summary>
public class SftpService : ISftpService
{
    private readonly string _rclonePath;
    private readonly IConfigManager _configManager;

    public string RclonePath => _rclonePath;

    public SftpService(IConfigManager configManager)
    {
        _configManager = configManager;
        
        // Use bundled rclone or custom path from settings
        var customPath = _configManager.Settings?.CustomRclonePath;
        if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
        {
            _rclonePath = customPath;
        }
        else
        {
            // Bundled rclone in app directory
            var appDir = AppContext.BaseDirectory;
            _rclonePath = Path.Combine(appDir, "rclone", "rclone.exe");
        }
    }

    public async Task<string> GetVersionAsync()
    {
        var result = await RunRcloneAsync("version");
        if (result.Success && !string.IsNullOrEmpty(result.Output))
        {
            var firstLine = result.Output.Split('\n').FirstOrDefault();
            return firstLine ?? "Unknown";
        }
        return "Unknown";
    }

    public async Task<string> ObscurePasswordAsync(string password)
    {
        var result = await RunRcloneAsync($"obscure \"{password}\"");
        return result.Output?.Trim() ?? string.Empty;
    }

    public async Task<bool> CreateRemoteAsync(SftpConnection connection)
    {
        var args = new StringBuilder();
        args.Append($"config create \"{connection.RcloneRemoteName}\" sftp ");
        args.Append($"host=\"{connection.Host}\" ");
        args.Append($"port={connection.Port} ");
        args.Append($"user=\"{connection.Username}\" ");

        if (connection.AuthType == AuthenticationType.Password && !string.IsNullOrEmpty(connection.ObscuredPassword))
        {
            args.Append($"pass=\"{connection.ObscuredPassword}\" ");
        }
        else if (connection.AuthType == AuthenticationType.KeyFile && !string.IsNullOrEmpty(connection.KeyFilePath))
        {
            args.Append($"key_file=\"{connection.KeyFilePath}\" ");
            if (!string.IsNullOrEmpty(connection.ObscuredKeyPassphrase))
            {
                args.Append($"key_file_pass=\"{connection.ObscuredKeyPassphrase}\" ");
            }
        }

        var result = await RunRcloneAsync(args.ToString());
        return result.Success;
    }

    public async Task<bool> DeleteRemoteAsync(string remoteName)
    {
        var result = await RunRcloneAsync($"config delete \"{remoteName}\"");
        return result.Success;
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(SftpConnection connection)
    {
        // First ensure remote is configured
        await CreateRemoteAsync(connection);

        // Try to list the remote path
        var remotePath = $"{connection.RcloneRemoteName}:{connection.RemotePath}";
        var result = await RunRcloneAsync($"lsd \"{remotePath}\" --max-depth 1", timeoutSeconds: 30);

        if (result.Success)
        {
            return (true, "Connection successful!");
        }
        else
        {
            return (false, result.Error ?? "Connection failed");
        }
    }

    public async Task<MountResult> MountAsync(SftpConnection connection, string driveLetter, CancellationToken cancellationToken = default)
    {
        // Ensure remote is configured
        await CreateRemoteAsync(connection);

        var settings = connection.MountSettings;
        var globalSettings = _configManager.Settings?.GlobalVfsSettings ?? new GlobalVfsSettings();
        
        // Always use global VFS settings
        var effectiveSettings = globalSettings.ToMountSettings();
        
        var remotePath = $"{connection.RcloneRemoteName}:{connection.RemotePath}";
        
        var args = new StringBuilder();
        args.Append($"mount \"{remotePath}\" {driveLetter}: ");
        
        // Mount options
        if (settings.NetworkMode)
        {
            args.Append("--network-mode ");
        }
        
        if (!string.IsNullOrEmpty(settings.VolumeName))
        {
            args.Append($"--volname \"{settings.VolumeName}\" ");
        }
        else
        {
            args.Append($"--volname \"{connection.Name}\" ");
        }
        
        if (settings.ReadOnly)
        {
            args.Append("--read-only ");
        }
        
        // VFS cache settings
        args.Append($"--vfs-cache-mode {effectiveSettings.CacheMode.ToString().ToLower()} ");
        
        if (!string.IsNullOrEmpty(effectiveSettings.CacheMaxSize))
        {
            args.Append($"--vfs-cache-max-size {effectiveSettings.CacheMaxSize} ");
        }
        
        if (!string.IsNullOrEmpty(effectiveSettings.CacheMaxAge))
        {
            args.Append($"--vfs-cache-max-age {effectiveSettings.CacheMaxAge} ");
        }
        
        if (effectiveSettings.CacheMaxFiles > 0)
        {
            args.Append($"--vfs-cache-max-files {effectiveSettings.CacheMaxFiles} ");
        }
        
        args.Append($"--dir-cache-time {effectiveSettings.DirCacheTimeMinutes}m ");
        
        if (effectiveSettings.PollInterval > 0)
        {
            args.Append($"--poll-interval {effectiveSettings.PollInterval}s ");
        }
        
        // Performance settings
        if (!string.IsNullOrEmpty(effectiveSettings.BufferSize))
        {
            args.Append($"--buffer-size {effectiveSettings.BufferSize} ");
        }
        
        if (!string.IsNullOrEmpty(effectiveSettings.ChunkSize))
        {
            args.Append($"--vfs-read-chunk-size {effectiveSettings.ChunkSize} ");
        }
        
        args.Append($"--transfers {effectiveSettings.Transfers} ");
        args.Append($"--checkers {effectiveSettings.Checkers} ");
        
        // Advanced settings
        if (effectiveSettings.AsyncRead)
        {
            args.Append("--vfs-read-wait 0 ");
        }
        
        if (effectiveSettings.AsyncWrite)
        {
            args.Append("--vfs-write-wait 0 ");
        }
        
        if (!string.IsNullOrEmpty(effectiveSettings.Umask) && effectiveSettings.Umask != "000")
        {
            args.Append($"--umask {effectiveSettings.Umask} ");
        }
        
        if (effectiveSettings.UID > 0)
        {
            args.Append($"--uid {effectiveSettings.UID} ");
        }
        
        if (effectiveSettings.GID > 0)
        {
            args.Append($"--gid {effectiveSettings.GID} ");
        }
        
        // Additional recommended options
        args.Append("--vfs-cache-poll-interval 1m ");
        args.Append("--log-level INFO ");

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _rclonePath,
                    Arguments = args.ToString(),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.Start();

            // Wait a bit to see if mount fails immediately
            await Task.Delay(2000, cancellationToken);

            if (process.HasExited)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                return new MountResult
                {
                    Success = false,
                    ErrorMessage = error,
                    DriveLetter = driveLetter
                };
            }

            // Check if drive is accessible
            if (Directory.Exists($"{driveLetter}:\\"))
            {
                return new MountResult
                {
                    Success = true,
                    Process = process,
                    DriveLetter = driveLetter
                };
            }

            // Wait a bit more for mount to complete
            await Task.Delay(3000, cancellationToken);

            if (Directory.Exists($"{driveLetter}:\\"))
            {
                return new MountResult
                {
                    Success = true,
                    Process = process,
                    DriveLetter = driveLetter
                };
            }

            return new MountResult
            {
                Success = false,
                ErrorMessage = "Mount did not become accessible in time",
                DriveLetter = driveLetter
            };
        }
        catch (Exception ex)
        {
            return new MountResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                DriveLetter = driveLetter
            };
        }
    }

    public async Task<bool> UnmountAsync(string driveLetter)
    {
        // On Windows, we use fusermount equivalent or kill the process
        // rclone doesn't have a built-in unmount command for Windows
        // We need to find and kill the rclone process for this drive
        
        var processes = Process.GetProcessesByName("rclone");
        foreach (var process in processes)
        {
            try
            {
                var commandLine = await GetProcessCommandLineAsync(process.Id);
                if (commandLine?.Contains($"{driveLetter}:") == true)
                {
                    process.Kill();
                    await process.WaitForExitAsync();
                    return true;
                }
            }
            catch
            {
                // Process may have already exited
            }
        }

        return false;
    }

    public List<string> GetAvailableDriveLetters()
    {
        var usedDrives = DriveInfo.GetDrives()
            .Select(d => d.Name[0].ToString().ToUpper())
            .ToHashSet();

        var allLetters = Enumerable.Range('A', 26)
            .Select(c => ((char)c).ToString())
            .ToList();

        // Prefer letters from end of alphabet for network drives
        return allLetters
            .Where(l => !usedDrives.Contains(l))
            .Reverse()
            .ToList();
    }

    private async Task<RcloneResult> RunRcloneAsync(string arguments, int timeoutSeconds = 60)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _rclonePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            
            var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
            var errorTask = process.StandardError.ReadToEndAsync(cts.Token);

            await process.WaitForExitAsync(cts.Token);

            var output = await outputTask;
            var error = await errorTask;

            return new RcloneResult
            {
                Success = process.ExitCode == 0,
                Output = output,
                Error = error,
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            return new RcloneResult
            {
                Success = false,
                Error = ex.Message,
                ExitCode = -1
            };
        }
    }

    private async Task<string?> GetProcessCommandLineAsync(int processId)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = $"process where processid={processId} get commandline",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output;
        }
        catch
        {
            return null;
        }
    }

    private class RcloneResult
    {
        public bool Success { get; init; }
        public string? Output { get; init; }
        public string? Error { get; init; }
        public int ExitCode { get; init; }
    }
}
