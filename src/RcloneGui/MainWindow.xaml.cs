using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.Services;
using RcloneGui.ViewModels;
using RcloneGui.Views;

namespace RcloneGui;

/// <summary>
/// Main application window with navigation.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IConfigManager _configManager;
    private bool _isClosing;

    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        _configManager = App.Services.GetRequiredService<IConfigManager>();
        
        // Set window size
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
        appWindow.Title = "Rclone GUI - SFTP Drive Mounter";

        // Handle close button
        appWindow.Closing += AppWindow_Closing;

        // Set up tray icon left click
        TrayIcon.LeftClickCommand = new RelayCommand(ShowAndActivateWindow);

        // Set initial page
        ContentFrame.Navigate(typeof(DrivesView));

        // Initialize
        _ = InitializeAsync();
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
                    ContentFrame.Navigate(typeof(AddConnectionView));
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
    }

    private void TrayShow_Click(object sender, RoutedEventArgs e)
    {
        ShowAndActivateWindow();
    }

    private void ShowAndActivateWindow()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Windows.Win32.PInvoke.ShowWindow(new Windows.Win32.Foundation.HWND(hwnd), Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
        Windows.Win32.PInvoke.SetForegroundWindow(new Windows.Win32.Foundation.HWND(hwnd));
        Activate();
    }

    private async void TrayMountAll_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.MountAllCommand.ExecuteAsync(null);
    }

    private async void TrayUnmountAll_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.UnmountAllCommand.ExecuteAsync(null);
    }

    private async void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;
        TrayIcon.Dispose();
        await App.ExitApplicationAsync();
    }

    public void NavigateToAddConnection(Models.SftpConnection? connectionToEdit = null)
    {
        ContentFrame.Navigate(typeof(AddConnectionView), connectionToEdit);
        
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
