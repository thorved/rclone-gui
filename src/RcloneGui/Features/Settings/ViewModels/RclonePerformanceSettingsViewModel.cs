using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Core.Models;
using RcloneGui.Core.Services;

namespace RcloneGui.Features.Settings.ViewModels;

/// <summary>
/// ViewModel for the global rclone performance settings page.
/// </summary>
public partial class RclonePerformanceSettingsViewModel : ObservableObject
{
    private readonly IConfigManager _configManager;
    private bool _isInitializing;

    #region VFS Cache Settings

    [ObservableProperty]
    private VfsCacheMode _cacheMode;

    [ObservableProperty]
    private string _cacheMaxSize = string.Empty;

    [ObservableProperty]
    private string? _cacheMaxAge;

    [ObservableProperty]
    private int _cacheMaxFiles;

    #endregion

    #region Directory Caching

    [ObservableProperty]
    private int _dirCacheTimeMinutes;

    [ObservableProperty]
    private int _pollInterval;

    #endregion

    #region Transfer Performance

    [ObservableProperty]
    private string _bufferSize = string.Empty;

    [ObservableProperty]
    private string _chunkSize = string.Empty;

    [ObservableProperty]
    private int _transfers;

    [ObservableProperty]
    private int _checkers;

    #endregion

    #region Advanced Settings

    [ObservableProperty]
    private bool _asyncRead;

    [ObservableProperty]
    private bool _asyncWrite;

    [ObservableProperty]
    private string _umask = string.Empty;

    [ObservableProperty]
    private int _uid;

    [ObservableProperty]
    private int _gid;

    #endregion

    #region Profile

    [ObservableProperty]
    private VfsPerformanceProfile _selectedProfile;

    #endregion

    /// <summary>
    /// Preview of the mount command with current settings.
    /// </summary>
    public string MountCommandPreview
    {
        get
        {
            var args = new List<string>();

            // VFS Cache
            args.Add($"--vfs-cache-mode {CacheMode.ToString().ToLower()}");
            args.Add($"--vfs-cache-max-size {CacheMaxSize}");
            if (!string.IsNullOrEmpty(CacheMaxAge))
                args.Add($"--vfs-cache-max-age {CacheMaxAge}");
            if (CacheMaxFiles > 0)
                args.Add($"--vfs-cache-max-files {CacheMaxFiles}");

            // Directory Cache
            args.Add($"--dir-cache-time {DirCacheTimeMinutes}m");
            if (PollInterval > 0)
                args.Add($"--poll-interval {PollInterval}s");

            // Performance
            args.Add($"--buffer-size {BufferSize}");
            args.Add($"--vfs-read-chunk-size {ChunkSize}");
            args.Add($"--transfers {Transfers}");
            args.Add($"--checkers {Checkers}");

            // Advanced
            if (AsyncRead)
                args.Add("--vfs-read-wait 0");
            if (AsyncWrite)
                args.Add("--vfs-write-wait 0");
            if (!string.IsNullOrEmpty(Umask) && Umask != "000")
                args.Add($"--umask {Umask}");
            if (Uid > 0)
                args.Add($"--uid {Uid}");
            if (Gid > 0)
                args.Add($"--gid {Gid}");

            return $"rclone mount remote: X: {string.Join(" ", args)}";
        }
    }

    public RclonePerformanceSettingsViewModel(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            var settings = _configManager.Settings?.GlobalVfsSettings ?? new GlobalVfsSettings();

            // Load all properties
            CacheMode = settings.CacheMode;
            CacheMaxSize = settings.CacheMaxSize;
            CacheMaxAge = settings.CacheMaxAge;
            CacheMaxFiles = settings.CacheMaxFiles;
            DirCacheTimeMinutes = settings.DirCacheTimeMinutes;
            PollInterval = settings.PollInterval;
            BufferSize = settings.BufferSize;
            ChunkSize = settings.ChunkSize;
            Transfers = settings.Transfers;
            Checkers = settings.Checkers;
            AsyncRead = settings.AsyncRead;
            AsyncWrite = settings.AsyncWrite;
            Umask = settings.Umask;
            Uid = settings.UID;
            Gid = settings.GID;
            SelectedProfile = settings.PerformanceProfile;
        }
        finally
        {
            _isInitializing = false;
        }
    }

    public async Task SaveAsync()
    {
        var settings = _configManager.Settings;
        if (settings == null) return;

        settings.GlobalVfsSettings = new GlobalVfsSettings
        {
            CacheMode = CacheMode,
            CacheMaxSize = CacheMaxSize,
            CacheMaxAge = CacheMaxAge,
            CacheMaxFiles = CacheMaxFiles,
            DirCacheTimeMinutes = DirCacheTimeMinutes,
            PollInterval = PollInterval,
            BufferSize = BufferSize,
            ChunkSize = ChunkSize,
            Transfers = Transfers,
            Checkers = Checkers,
            AsyncRead = AsyncRead,
            AsyncWrite = AsyncWrite,
            Umask = Umask,
            UID = Uid,
            GID = Gid,
            PerformanceProfile = SelectedProfile,
            IsCustomized = true
        };

        await _configManager.SaveSettingsAsync();
    }

    public void ResetToDefaults()
    {
        var defaults = new GlobalVfsSettings();
        
        CacheMode = defaults.CacheMode;
        CacheMaxSize = defaults.CacheMaxSize;
        CacheMaxAge = defaults.CacheMaxAge;
        CacheMaxFiles = defaults.CacheMaxFiles;
        DirCacheTimeMinutes = defaults.DirCacheTimeMinutes;
        PollInterval = defaults.PollInterval;
        BufferSize = defaults.BufferSize;
        ChunkSize = defaults.ChunkSize;
        Transfers = defaults.Transfers;
        Checkers = defaults.Checkers;
        AsyncRead = defaults.AsyncRead;
        AsyncWrite = defaults.AsyncWrite;
        Umask = defaults.Umask;
        Uid = defaults.UID;
        Gid = defaults.GID;
        SelectedProfile = defaults.PerformanceProfile;
    }

    public void ApplyPerformanceProfile(VfsPerformanceProfile profile)
    {
        var tempSettings = new GlobalVfsSettings();
        profile.ApplyTo(tempSettings);

        CacheMode = tempSettings.CacheMode;
        CacheMaxSize = tempSettings.CacheMaxSize;
        CacheMaxAge = tempSettings.CacheMaxAge;
        CacheMaxFiles = tempSettings.CacheMaxFiles;
        DirCacheTimeMinutes = tempSettings.DirCacheTimeMinutes;
        PollInterval = tempSettings.PollInterval;
        BufferSize = tempSettings.BufferSize;
        ChunkSize = tempSettings.ChunkSize;
        Transfers = tempSettings.Transfers;
        Checkers = tempSettings.Checkers;
        AsyncRead = tempSettings.AsyncRead;
        AsyncWrite = tempSettings.AsyncWrite;
        Umask = tempSettings.Umask;
        Uid = tempSettings.UID;
        Gid = tempSettings.GID;
        SelectedProfile = profile;
    }

    // Trigger property changed for MountCommandPreview when any setting changes
    partial void OnCacheModeChanged(VfsCacheMode value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnCacheMaxSizeChanged(string value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnCacheMaxAgeChanged(string? value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnCacheMaxFilesChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnDirCacheTimeMinutesChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnPollIntervalChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnBufferSizeChanged(string value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnChunkSizeChanged(string value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnTransfersChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnCheckersChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnAsyncReadChanged(bool value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnAsyncWriteChanged(bool value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnUmaskChanged(string value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnUidChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
    partial void OnGidChanged(int value) => OnPropertyChanged(nameof(MountCommandPreview));
}
