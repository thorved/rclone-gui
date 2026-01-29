using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using RcloneGui.Services;
using RcloneGui.ViewModels;
using System.Threading;

namespace RcloneGui;

/// <summary>
/// Main application entry point with single instance enforcement and DI setup.
/// </summary>
public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;
    private const string MutexName = "RcloneGui_SingleInstance_Mutex";
    
    private Window? _mainWindow;
    private readonly IHost _host;

    public static IServiceProvider Services => ((App)Current)._host.Services;
    
    public static MainWindow? MainWindowInstance => ((App)Current)._mainWindow as MainWindow;

    public App()
    {
        // Ensure single instance
        _singleInstanceMutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            // Another instance is running - signal it and exit
            // TODO: Use named pipe to bring existing window to foreground
            Environment.Exit(0);
            return;
        }

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IRcloneService, RcloneService>();
                services.AddSingleton<IConfigManager, ConfigManager>();
                services.AddSingleton<IMountManager, MountManager>();
                services.AddSingleton<IWinFspManager, WinFspManager>();
                services.AddSingleton<INotificationService, NotificationService>();
                
                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<AddConnectionViewModel>();
                services.AddTransient<AddFtpConnectionViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<DriveItemViewModel>();
            })
            .Build();

        InitializeComponent();
    }

    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Initialize services
        var configManager = Services.GetRequiredService<IConfigManager>();
        await configManager.InitializeAsync();
        
        // Check WinFsp installation
        var winFspManager = Services.GetRequiredService<IWinFspManager>();
        if (!winFspManager.IsInstalled())
        {
            // Will prompt user to install WinFsp
        }

        _mainWindow = new MainWindow();
        
        // Apply saved theme on startup
        ApplyTheme(configManager.Settings?.Theme ?? Models.AppTheme.System);
        
        // Check if should start minimized
        var shouldStartMinimized = ShouldStartMinimized(configManager.Settings);
        
        if (shouldStartMinimized)
        {
            // Don't activate window, just start in background with tray icon
            // Window is created but hidden
        }
        else
        {
            _mainWindow.Activate();
        }
        
        // Auto-mount configured drives
        var mountManager = Services.GetRequiredService<IMountManager>();
        await mountManager.AutoMountAsync();
    }

    private static bool ShouldStartMinimized(Models.AppSettings? settings)
    {
        // Check command line arguments for --minimized flag
        var cmdArgs = Environment.GetCommandLineArgs();
        if (cmdArgs.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        // Check settings (only if StartWithWindows is also enabled, for manual launches respect the setting)
        return settings?.StartMinimized == true;
    }

    private static void ApplyTheme(Models.AppTheme theme)
    {
        var window = MainWindowInstance;
        if (window?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme switch
            {
                Models.AppTheme.Light => Microsoft.UI.Xaml.ElementTheme.Light,
                Models.AppTheme.Dark => Microsoft.UI.Xaml.ElementTheme.Dark,
                _ => Microsoft.UI.Xaml.ElementTheme.Default
            };
        }
    }

    public static void ShowWindow()
    {
        var window = MainWindowInstance;
        if (window != null)
        {
            window.Activate();
        }
    }
}
