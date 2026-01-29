using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Models;
using RcloneGui.Services;
using System.Collections.ObjectModel;

namespace RcloneGui.ViewModels;

/// <summary>
/// ViewModel for adding/editing FTP connections.
/// </summary>
public partial class AddFtpConnectionViewModel : ObservableObject
{
    private readonly IRcloneService _rcloneService;
    private readonly IConfigManager _configManager;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _host = string.Empty;

    [ObservableProperty]
    private int _port = 21;

    [ObservableProperty]
    private bool _isAnonymous = false;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private FtpTlsMode _tlsMode = FtpTlsMode.None;

    [ObservableProperty]
    private bool _passiveMode = true;

    [ObservableProperty]
    private string _remotePath = "/";

    [ObservableProperty]
    private string _selectedDriveLetter = string.Empty;

    [ObservableProperty]
    private bool _networkMode = true;

    [ObservableProperty]
    private string _volumeName = string.Empty;

    [ObservableProperty]
    private bool _readOnly = false;

    [ObservableProperty]
    private VfsCacheMode _cacheMode = VfsCacheMode.Writes;

    [ObservableProperty]
    private string _cacheMaxSize = "10G";

    [ObservableProperty]
    private int _dirCacheTimeMinutes = 5;

    [ObservableProperty]
    private bool _autoMount = false;

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _testResult;

    [ObservableProperty]
    private bool _testSuccess;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<string> _availableDriveLetters = new();

    public ObservableCollection<FtpTlsMode> TlsModes { get; } = new(Enum.GetValues<FtpTlsMode>());

    public ObservableCollection<VfsCacheMode> CacheModes { get; } = new(Enum.GetValues<VfsCacheMode>());

    private string? _editingConnectionId;

    public AddFtpConnectionViewModel(IRcloneService rcloneService, IConfigManager configManager)
    {
        _rcloneService = rcloneService;
        _configManager = configManager;
        
        LoadAvailableDriveLetters();
    }

    public void LoadConnection(FtpConnection connection)
    {
        System.Diagnostics.Debug.WriteLine($"LoadConnection called for: {connection.Name}");
        _editingConnectionId = connection.Id;
        Name = connection.Name;
        Host = connection.Host;
        Port = connection.Port;
        IsAnonymous = connection.IsAnonymous;
        Username = connection.Username;
        TlsMode = connection.TlsMode;
        PassiveMode = connection.PassiveMode;
        RemotePath = connection.RemotePath;
        SelectedDriveLetter = connection.MountSettings.DriveLetter ?? string.Empty;
        NetworkMode = connection.MountSettings.NetworkMode;
        VolumeName = connection.MountSettings.VolumeName ?? string.Empty;
        ReadOnly = connection.MountSettings.ReadOnly;
        CacheMode = connection.MountSettings.CacheMode;
        CacheMaxSize = connection.MountSettings.CacheMaxSize;
        DirCacheTimeMinutes = connection.MountSettings.DirCacheTimeMinutes;
        AutoMount = connection.AutoMount;
        System.Diagnostics.Debug.WriteLine($"After load - Name: {Name}, Host: {Host}, IsAnonymous: {IsAnonymous}");
        // Note: Password fields are not loaded for security
    }

    public void ResetForm()
    {
        _editingConnectionId = null;
        Name = string.Empty;
        Host = string.Empty;
        Port = 21;
        IsAnonymous = false;
        Username = string.Empty;
        Password = string.Empty;
        TlsMode = FtpTlsMode.None;
        PassiveMode = true;
        RemotePath = "/";
        SelectedDriveLetter = string.Empty;
        NetworkMode = true;
        VolumeName = string.Empty;
        ReadOnly = false;
        CacheMode = VfsCacheMode.Writes;
        CacheMaxSize = "10G";
        DirCacheTimeMinutes = 5;
        AutoMount = false;
        TestResult = null;
        TestSuccess = false;
        ErrorMessage = null;
    }

    public void RefreshAvailableDriveLetters() => LoadAvailableDriveLetters();

    private void LoadAvailableDriveLetters()
    {
        AvailableDriveLetters.Clear();
        AvailableDriveLetters.Add(string.Empty); // Auto-assign option
        
        foreach (var letter in _rcloneService.GetAvailableDriveLetters())
        {
            AvailableDriveLetters.Add(letter);
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            ErrorMessage = "Host is required";
            return;
        }

        if (!IsAnonymous && string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required for non-anonymous connections";
            return;
        }

        IsTesting = true;
        TestResult = null;
        ErrorMessage = null;

        try
        {
            var testConnection = CreateConnection();
            
            // Obscure password for test (only if not anonymous)
            if (!IsAnonymous && !string.IsNullOrEmpty(Password))
            {
                testConnection.ObscuredPassword = await _rcloneService.ObscurePasswordAsync(Password);
            }

            var (success, message) = await _rcloneService.TestFtpConnectionAsync(testConnection);
            TestSuccess = success;
            TestResult = message;

            if (!success)
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            TestSuccess = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    public async Task<bool> SaveAsync()
    {
        if (!Validate())
        {
            return false;
        }

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var connection = CreateConnection();
            
            // Obscure password (only if not anonymous)
            if (!IsAnonymous && !string.IsNullOrEmpty(Password))
            {
                connection.ObscuredPassword = await _rcloneService.ObscurePasswordAsync(Password);
            }

            // Create rclone remote
            var created = await _rcloneService.CreateFtpRemoteAsync(connection);
            if (!created)
            {
                ErrorMessage = "Failed to create rclone configuration";
                return false;
            }

            // Save to config
            if (_editingConnectionId != null)
            {
                connection.Id = _editingConnectionId;
                await _configManager.UpdateFtpConnectionAsync(connection);
            }
            else
            {
                await _configManager.AddFtpConnectionAsync(connection);
            }

            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private FtpConnection CreateConnection()
    {
        return new FtpConnection
        {
            Id = _editingConnectionId ?? Guid.NewGuid().ToString(),
            Name = Name.Trim(),
            Host = Host.Trim(),
            Port = Port,
            Username = IsAnonymous ? string.Empty : Username.Trim(),
            TlsMode = TlsMode,
            PassiveMode = PassiveMode,
            RemotePath = string.IsNullOrWhiteSpace(RemotePath) ? "/" : RemotePath.Trim(),
            AutoMount = AutoMount,
            MountSettings = new MountSettings
            {
                DriveLetter = SelectedDriveLetter,
                NetworkMode = NetworkMode,
                VolumeName = string.IsNullOrWhiteSpace(VolumeName) ? null : VolumeName.Trim(),
                ReadOnly = ReadOnly,
                CacheMode = CacheMode,
                CacheMaxSize = CacheMaxSize,
                DirCacheTimeMinutes = DirCacheTimeMinutes
            }
        };
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Connection name is required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            ErrorMessage = "Host is required";
            return false;
        }

        if (!IsAnonymous && string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required for non-anonymous connections";
            return false;
        }

        if (Port < 1 || Port > 65535)
        {
            ErrorMessage = "Port must be between 1 and 65535";
            return false;
        }

        if (!IsAnonymous && string.IsNullOrEmpty(Password) && _editingConnectionId == null)
        {
            ErrorMessage = "Password is required for non-anonymous connections";
            return false;
        }

        return true;
    }

    partial void OnTlsModeChanged(FtpTlsMode value)
    {
        // Auto-adjust port based on TLS mode
        if (value == FtpTlsMode.Implicit && Port == 21)
        {
            Port = 990;
        }
        else if (value == FtpTlsMode.None && Port == 990)
        {
            Port = 21;
        }
    }
}
