using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.Services;
using RcloneGui.ViewModels;
using RcloneGui.Views;
using RcloneGui.Views.ConnectionType.Sftp;
using Windows.Graphics;

namespace RcloneGui;

/// <summary>
/// Main application window with navigation.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IConfigManager _configManager;
    private bool _isClosing;
    private AppWindow _appWindow;

    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        _configManager = App.Services.GetRequiredService<IConfigManager>();
        
        // Get AppWindow for customization
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        
        // Set window icon
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
        if (File.Exists(iconPath))
        {
            _appWindow.SetIcon(iconPath);
        }
        
        // Set fixed window size (larger)
        _appWindow.Resize(new SizeInt32(900, 650));
        _appWindow.Title = "Rclone GUI";
        
        // Configure custom title bar
        ConfigureTitleBar();
        
        // Disable resizing but allow maximize by using OverlappedPresenter
        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = true;
            presenter.IsMinimizable = true;
        }

        // Handle close button
        _appWindow.Closing += AppWindow_Closing;

        // Set up tray icon left click
        TrayIcon.LeftClickCommand = new RelayCommand(ShowAndActivateWindow);
        
        // Set up tray context menu commands
        TrayShowMenuItem.Command = new RelayCommand(ShowAndActivateWindow);
        TrayMountAllMenuItem.Command = new AsyncRelayCommand(async () => await _viewModel.MountAllCommand.ExecuteAsync(null));
        TrayUnmountAllMenuItem.Command = new AsyncRelayCommand(async () => await _viewModel.UnmountAllCommand.ExecuteAsync(null));
        TrayExitMenuItem.Command = new RelayCommand(ExitApplication);

        // Set initial page
        ContentFrame.Navigate(typeof(DrivesView));

        // Initialize
        _ = InitializeAsync();
    }

    private void ConfigureTitleBar()
    {
        // Extend content into the title bar
        ExtendsContentIntoTitleBar = true;
        
        // Set the custom title bar element
        SetTitleBar(AppTitleBar);
        
        // Customize title bar colors
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = _appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            
            // Set dark colors
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(50, 255, 255, 255);
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(80, 255, 255, 255);
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(128, 255, 255, 255);
            titleBar.ButtonHoverForegroundColor = Colors.White;
            titleBar.ButtonPressedForegroundColor = Colors.White;
        }
    }

    private async Task InitializeAsync()
    {
        await _viewModel.InitializeAsync();

        // Check WinFsp and show warning if not installed
        if (!_viewModel.IsWinFspInstalled)
        {
            await ShowWinFspWarningAsync();
        }
    }

    private async Task ShowWinFspWarningAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "WinFsp Required",
            Content = "WinFsp is required to mount drives. Would you like to install it now?",
            PrimaryButtonText = "Install",
            SecondaryButtonText = "Download",
            CloseButtonText = "Later",
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await _viewModel.InstallWinFspCommand.ExecuteAsync(null);
        }
        else if (result == ContentDialogResult.Secondary)
        {
            var winFspManager = App.Services.GetRequiredService<IWinFspManager>();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = winFspManager.GetDownloadUrl(),
                UseShellExecute = true
            });
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            switch (tag)
            {
                case "drives":
                    ContentFrame.Navigate(typeof(DrivesView));
                    break;
                case "add":
                    // Show connection type selector for new connections
                    ContentFrame.Navigate(typeof(ConnectionTypeView));
                    break;
                case "settings":
                    ContentFrame.Navigate(typeof(SettingsView));
                    break;
            }
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(SettingsView));
        
        // Update navigation selection
        foreach (var item in NavView.FooterMenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "settings")
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_isClosing) return;

        var settings = _configManager.Settings;
        if (settings?.MinimizeToTray == true)
        {
            args.Cancel = true;
            // Hide window but keep running in tray
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Windows.Win32.PInvoke.ShowWindow(new Windows.Win32.Foundation.HWND(hwnd), Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_HIDE);
        }
        else
        {
            // Not minimizing to tray - perform exit with unmount
            args.Cancel = true;
            ExitApplication();
        }
    }

    private void ShowAndActivateWindow()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Windows.Win32.PInvoke.ShowWindow(new Windows.Win32.Foundation.HWND(hwnd), Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
        Windows.Win32.PInvoke.SetForegroundWindow(new Windows.Win32.Foundation.HWND(hwnd));
        Activate();
    }

    private void ExitApplication()
    {
        // Show the window first so we can display a dialog
        ShowAndActivateWindow();
        
        // Use dispatcher to show dialog on UI thread
        DispatcherQueue.TryEnqueue(async () =>
        {
            await ShowExitConfirmationAsync();
        });
    }

    private async Task ShowExitConfirmationAsync()
    {
        var mountManager = App.Services.GetRequiredService<IMountManager>();
        var mountedDrives = mountManager.MountedDrives;
        var shouldUnmount = _configManager.Settings?.UnmountOnClose ?? true;
        
        string message;
        if (mountedDrives.Count > 0 && shouldUnmount)
        {
            message = $"Closing the application will unmount {mountedDrives.Count} drive(s).\n\nAre you sure you want to exit?";
        }
        else if (mountedDrives.Count > 0 && !shouldUnmount)
        {
            message = $"You have {mountedDrives.Count} drive(s) mounted. They will remain mounted after exit.\n\nAre you sure you want to exit?";
        }
        else
        {
            message = "Are you sure you want to exit?";
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Exit Rclone GUI",
            Content = message,
            PrimaryButtonText = "Exit",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            await PerformExitAsync();
        }
    }

    private async Task PerformExitAsync()
    {
        _isClosing = true;
        
        var shouldUnmount = _configManager.Settings?.UnmountOnClose ?? true;
        
        if (shouldUnmount)
        {
            try
            {
                var mountManager = App.Services.GetRequiredService<IMountManager>();
                var mountedDrives = mountManager.MountedDrives;
                
                if (mountedDrives.Count > 0)
                {
                    // Show a progress message while unmounting
                    // Update UI to show we're unmounting
                    
                    // Unmount all drives
                    await mountManager.UnmountAllAsync();
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        
        // Dispose tray icon
        try
        {
            TrayIcon.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
        
        // Exit the application
        Environment.Exit(0);
    }

    public void NavigateToAddConnection(Models.SftpConnection? connectionToEdit = null)
    {
        // Clear cache to ensure fresh page with new parameters
        ContentFrame.BackStack.Clear();
        ContentFrame.ForwardStack.Clear();
        
        if (connectionToEdit != null)
        {
            // Editing existing connection - go directly to SftpConnectionView
            System.Diagnostics.Debug.WriteLine($"NavigateToAddConnection: Editing {connectionToEdit.Name}");
            ContentFrame.Navigate(typeof(SftpConnectionView), connectionToEdit);
        }
        else
        {
            // Adding new connection - show connection type selector first
            System.Diagnostics.Debug.WriteLine("NavigateToAddConnection: Showing type selector");
            ContentFrame.Navigate(typeof(ConnectionTypeView));
        }
        
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "add")
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
    }

    /// <summary>
    /// Navigate to SftpConnectionView for new or editing SFTP connection.
    /// </summary>
    public void NavigateToSftpConnection(Models.SftpConnection? connectionToEdit)
    {
        ContentFrame.BackStack.Clear();
        ContentFrame.ForwardStack.Clear();
        ContentFrame.Navigate(typeof(SftpConnectionView), connectionToEdit);
        
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "add")
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
    }

    /// <summary>
    /// Navigate to FtpConnectionView for new or editing FTP connection.
    /// </summary>
    public void NavigateToFtpConnection(Models.FtpConnection? connectionToEdit)
    {
        ContentFrame.BackStack.Clear();
        ContentFrame.ForwardStack.Clear();
        ContentFrame.Navigate(typeof(Views.ConnectionType.Ftp.FtpConnectionView), connectionToEdit);
        
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "add")
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
    }

    public void NavigateToDrives()
    {
        ContentFrame.Navigate(typeof(DrivesView));
        
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "drives")
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
    }
}
