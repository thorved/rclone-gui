using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.Core.Models;
using RcloneGui.Features.Sftp.Models;
using RcloneGui.Core.Services;
using Windows.Storage.Pickers;

namespace RcloneGui.Features.Sftp.Views;

public sealed partial class SftpSettingsDialog : ContentDialog
{
    private readonly IConfigManager _configManager;
    private readonly ISftpService _sftpService;
    private SftpConnection? _connection;
    private bool _isEditing;
    private string? _originalPassword;
    private string? _originalKeyPassphrase;

    public SftpSettingsDialog()
    {
        this.InitializeComponent();
        _configManager = App.Services.GetRequiredService<IConfigManager>();
        _sftpService = App.Services.GetRequiredService<ISftpService>();
        
        // Initialize drive letters
        RefreshDriveLetters();
    }

    /// <summary>
    /// Load an existing connection for editing.
    /// </summary>
    public void LoadConnection(SftpConnection connection)
    {
        _connection = connection;
        _isEditing = true;
        _originalPassword = connection.ObscuredPassword;
        _originalKeyPassphrase = connection.ObscuredKeyPassphrase;

        Title = "Edit SFTP Connection";
        PasswordHintText.Visibility = Visibility.Visible;

        // Populate fields
        NameBox.Text = connection.Name;
        HostBox.Text = connection.Host;
        PortBox.Value = connection.Port;
        UsernameBox.Text = connection.Username;
        RemotePathBox.Text = connection.RemotePath;
        VolumeNameBox.Text = connection.MountSettings.VolumeName;
        ReadOnlySwitch.IsChecked = connection.MountSettings.ReadOnly;
        NetworkModeSwitch.IsChecked = connection.MountSettings.NetworkMode;
        AutoMountSwitch.IsChecked = connection.AutoMount;

        // Auth type
        if (connection.AuthType == AuthenticationType.Password)
        {
            PasswordRadio.IsChecked = true;
            PasswordPanel.Visibility = Visibility.Visible;
            KeyPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            KeyRadio.IsChecked = true;
            PasswordPanel.Visibility = Visibility.Collapsed;
            KeyPanel.Visibility = Visibility.Visible;
            KeyFileBox.Text = connection.KeyFilePath ?? "";
        }

        // Refresh drive letters and add current one back
        RefreshDriveLetters(connection.MountSettings.DriveLetter);
        
        // Select the current drive letter
        if (!string.IsNullOrEmpty(connection.MountSettings.DriveLetter))
        {
            DriveLetterCombo.SelectedItem = connection.MountSettings.DriveLetter;
        }
    }

    private void RefreshDriveLetters(string? includeLetter = null)
    {
        var usedDrives = DriveInfo.GetDrives().Select(d => d.Name[0].ToString()).ToHashSet();
        var available = new List<string>();

        for (char c = 'D'; c <= 'Z'; c++)
        {
            var letter = c.ToString();
            if (!usedDrives.Contains(letter) || letter == includeLetter)
            {
                available.Add(letter);
            }
        }

        DriveLetterCombo.ItemsSource = available;
        if (available.Count > 0 && DriveLetterCombo.SelectedItem == null)
        {
            DriveLetterCombo.SelectedIndex = 0;
        }
    }

    private void AuthType_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.IsChecked == true)
        {
            var tag = rb.Tag?.ToString();
            if (PasswordPanel == null || KeyPanel == null) return;

            if (tag == "Password")
            {
                PasswordPanel.Visibility = Visibility.Visible;
                KeyPanel.Visibility = Visibility.Collapsed;
            }
            else if (tag == "Key")
            {
                PasswordPanel.Visibility = Visibility.Collapsed;
                KeyPanel.Visibility = Visibility.Visible;
            }
        }
    }

    private async void BrowseKeyFile_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            KeyFileBox.Text = file.Path;
        }
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Defer to allow async validation
        var deferral = args.GetDeferral();

        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ShowError("Connection name is required");
                args.Cancel = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(HostBox.Text))
            {
                ShowError("Host is required");
                args.Cancel = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ShowError("Username is required");
                args.Cancel = true;
                return;
            }

            var isPasswordAuth = PasswordRadio.IsChecked == true;

            // For new connections, password is required
            if (!_isEditing && isPasswordAuth && string.IsNullOrEmpty(PasswordBox.Password))
            {
                ShowError("Password is required");
                args.Cancel = true;
                return;
            }

            // Create or update connection
            if (_isEditing && _connection != null)
            {
                await UpdateConnectionAsync();
            }
            else
            {
                await CreateConnectionAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save: {ex.Message}");
            args.Cancel = true;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private async Task CreateConnectionAsync()
    {
        var isPasswordAuth = PasswordRadio.IsChecked == true;
        
        // Obscure password
        string? obscuredPassword = null;
        if (isPasswordAuth && !string.IsNullOrEmpty(PasswordBox.Password))
        {
            obscuredPassword = await _sftpService.ObscurePasswordAsync(PasswordBox.Password);
        }

        string? obscuredKeyPassphrase = null;
        if (!isPasswordAuth && !string.IsNullOrEmpty(KeyPassphraseBox.Password))
        {
            obscuredKeyPassphrase = await _sftpService.ObscurePasswordAsync(KeyPassphraseBox.Password);
        }

        var connection = new SftpConnection
        {
            Name = NameBox.Text.Trim(),
            Host = HostBox.Text.Trim(),
            Port = (int)PortBox.Value,
            Username = UsernameBox.Text.Trim(),
            AuthType = isPasswordAuth ? AuthenticationType.Password : AuthenticationType.KeyFile,
            ObscuredPassword = obscuredPassword,
            KeyFilePath = isPasswordAuth ? null : KeyFileBox.Text,
            ObscuredKeyPassphrase = obscuredKeyPassphrase,
            RemotePath = string.IsNullOrWhiteSpace(RemotePathBox.Text) ? "/" : RemotePathBox.Text.Trim(),
            AutoMount = AutoMountSwitch.IsChecked == true,
            MountSettings = new MountSettings
            {
                DriveLetter = DriveLetterCombo.SelectedItem?.ToString() ?? "Z",
                VolumeName = string.IsNullOrWhiteSpace(VolumeNameBox.Text) ? "SFTP Drive" : VolumeNameBox.Text.Trim(),
                ReadOnly = ReadOnlySwitch.IsChecked == true,
                NetworkMode = NetworkModeSwitch.IsChecked == true
            }
        };

        // Create rclone remote config
        await _sftpService.CreateRemoteAsync(connection);
        await _configManager.AddConnectionAsync(connection);
    }

    private async Task UpdateConnectionAsync()
    {
        if (_connection == null) return;

        var isPasswordAuth = PasswordRadio.IsChecked == true;

        // Update connection properties
        _connection.Name = NameBox.Text.Trim();
        _connection.Host = HostBox.Text.Trim();
        _connection.Port = (int)PortBox.Value;
        _connection.Username = UsernameBox.Text.Trim();
        _connection.AuthType = isPasswordAuth ? AuthenticationType.Password : AuthenticationType.KeyFile;
        _connection.RemotePath = string.IsNullOrWhiteSpace(RemotePathBox.Text) ? "/" : RemotePathBox.Text.Trim();
        _connection.AutoMount = AutoMountSwitch.IsChecked == true;
        _connection.KeyFilePath = isPasswordAuth ? null : KeyFileBox.Text;

        // Handle password - only update if changed
        if (isPasswordAuth && !string.IsNullOrEmpty(PasswordBox.Password))
        {
            _connection.ObscuredPassword = await _sftpService.ObscurePasswordAsync(PasswordBox.Password);
        }
        else if (isPasswordAuth && string.IsNullOrEmpty(PasswordBox.Password))
        {
            // Keep original password
            _connection.ObscuredPassword = _originalPassword;
        }

        // Handle key passphrase
        if (!isPasswordAuth && !string.IsNullOrEmpty(KeyPassphraseBox.Password))
        {
            _connection.ObscuredKeyPassphrase = await _sftpService.ObscurePasswordAsync(KeyPassphraseBox.Password);
        }
        else if (!isPasswordAuth && string.IsNullOrEmpty(KeyPassphraseBox.Password))
        {
            _connection.ObscuredKeyPassphrase = _originalKeyPassphrase;
        }

        // Update mount settings
        _connection.MountSettings.DriveLetter = DriveLetterCombo.SelectedItem?.ToString() ?? "Z";
        _connection.MountSettings.VolumeName = string.IsNullOrWhiteSpace(VolumeNameBox.Text) ? "SFTP Drive" : VolumeNameBox.Text.Trim();
        _connection.MountSettings.ReadOnly = ReadOnlySwitch.IsChecked == true;
        _connection.MountSettings.NetworkMode = NetworkModeSwitch.IsChecked == true;

        // Update rclone remote config
        await _sftpService.CreateRemoteAsync(_connection);
        await _configManager.UpdateConnectionAsync(_connection);
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }
}
