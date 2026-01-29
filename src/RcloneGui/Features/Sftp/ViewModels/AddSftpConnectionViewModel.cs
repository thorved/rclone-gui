using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RcloneGui.Core.Models;
using RcloneGui.Features.Sftp.Models;
using RcloneGui.Core.Services;
using System.Collections.ObjectModel;

namespace RcloneGui.Features.Sftp.ViewModels;

/// <summary>
/// ViewModel for adding/editing SFTP connections.
/// </summary>
public partial class AddSftpConnectionViewModel : ObservableObject
{
    private readonly ISftpService _sftpService;
    private readonly IConfigManager _configManager;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _host = string.Empty;

    [ObservableProperty]
    private int _port = 22;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private AuthenticationType _authType = AuthenticationType.Password;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _keyFilePath = string.Empty;

    [ObservableProperty]
    private string _keyPassphrase = string.Empty;

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

    public bool IsPasswordAuth => AuthType == AuthenticationType.Password;
    public bool IsKeyAuth => AuthType == AuthenticationType.KeyFile;

    public ObservableCollection<VfsCacheMode> CacheModes { get; } = new(Enum.GetValues<VfsCacheMode>());

    private string? _editingConnectionId;

    public AddSftpConnectionViewModel(ISftpService sftpService, IConfigManager configManager)
    {
        _sftpService = sftpService;
        _configManager = configManager;
        
        LoadAvailableDriveLetters();
    }

    public void LoadConnection(SftpConnection connection)
    {
        System.Diagnostics.Debug.WriteLine($"LoadConnection called for: {connection.Name}");
        _editingConnectionId = connection.Id;
        Name = connection.Name;
        Host = connection.Host;
        Port = connection.Port;
        Username = connection.Username;
        AuthType = connection.AuthType;
        KeyFilePath = connection.KeyFilePath ?? string.Empty;
        RemotePath = connection.RemotePath;
        SelectedDriveLetter = connection.MountSettings.DriveLetter ?? string.Empty;
        NetworkMode = connection.MountSettings.NetworkMode;
        VolumeName = connection.MountSettings.VolumeName ?? string.Empty;
        ReadOnly = connection.MountSettings.ReadOnly;
        CacheMode = connection.MountSettings.CacheMode;
        CacheMaxSize = connection.MountSettings.CacheMaxSize;
        DirCacheTimeMinutes = connection.MountSettings.DirCacheTimeMinutes;
        AutoMount = connection.AutoMount;
        System.Diagnostics.Debug.WriteLine($"After load - Name: {Name}, Host: {Host}, Username: {Username}");
        // Note: Password fields are not loaded for security
    }

    public void ResetForm()
    {
        _editingConnectionId = null;
        Name = string.Empty;
        Host = string.Empty;
        Port = 22;
        Username = string.Empty;
        AuthType = AuthenticationType.Password;
        Password = string.Empty;
        KeyFilePath = string.Empty;
        KeyPassphrase = string.Empty;
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
        
        foreach (var letter in _sftpService.GetAvailableDriveLetters())
        {
            AvailableDriveLetters.Add(letter);
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(Host) || string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Host and username are required";
            return;
        }

        IsTesting = true;
        TestResult = null;
        ErrorMessage = null;

        try
        {
            var testConnection = CreateConnection();
            
            // Obscure password for test
            if (AuthType == AuthenticationType.Password && !string.IsNullOrEmpty(Password))
            {
                testConnection.ObscuredPassword = await _sftpService.ObscurePasswordAsync(Password);
            }
            else if (AuthType == AuthenticationType.KeyFile && !string.IsNullOrEmpty(KeyPassphrase))
            {
                testConnection.ObscuredKeyPassphrase = await _sftpService.ObscurePasswordAsync(KeyPassphrase);
            }

            var (success, message) = await _sftpService.TestConnectionAsync(testConnection);
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
            
            // Obscure password
            if (AuthType == AuthenticationType.Password && !string.IsNullOrEmpty(Password))
            {
                connection.ObscuredPassword = await _sftpService.ObscurePasswordAsync(Password);
            }
            else if (AuthType == AuthenticationType.KeyFile && !string.IsNullOrEmpty(KeyPassphrase))
            {
                connection.ObscuredKeyPassphrase = await _sftpService.ObscurePasswordAsync(KeyPassphrase);
            }

            // Create rclone remote
            var created = await _sftpService.CreateRemoteAsync(connection);
            if (!created)
            {
                ErrorMessage = "Failed to create rclone configuration";
                return false;
            }

            // Save to config
            if (_editingConnectionId != null)
            {
                connection.Id = _editingConnectionId;
                await _configManager.UpdateConnectionAsync(connection);
            }
            else
            {
                await _configManager.AddConnectionAsync(connection);
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

    private SftpConnection CreateConnection()
    {
        return new SftpConnection
        {
            Id = _editingConnectionId ?? Guid.NewGuid().ToString(),
            Name = Name.Trim(),
            Host = Host.Trim(),
            Port = Port,
            Username = Username.Trim(),
            AuthType = AuthType,
            KeyFilePath = AuthType == AuthenticationType.KeyFile ? KeyFilePath : null,
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

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required";
            return false;
        }

        if (Port < 1 || Port > 65535)
        {
            ErrorMessage = "Port must be between 1 and 65535";
            return false;
        }

        if (AuthType == AuthenticationType.Password && string.IsNullOrEmpty(Password) && _editingConnectionId == null)
        {
            ErrorMessage = "Password is required for password authentication";
            return false;
        }

        if (AuthType == AuthenticationType.KeyFile && string.IsNullOrWhiteSpace(KeyFilePath))
        {
            ErrorMessage = "Key file path is required for key authentication";
            return false;
        }

        return true;
    }

    partial void OnAuthTypeChanged(AuthenticationType value)
    {
        OnPropertyChanged(nameof(IsPasswordAuth));
        OnPropertyChanged(nameof(IsKeyAuth));
    }
}
