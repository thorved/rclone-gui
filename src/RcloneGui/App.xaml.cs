using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using RcloneGui.Services;
using RcloneGui.ViewModels;
using RcloneGui.Views;
using System.Threading;
using Windows.ApplicationModel.Activation;

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
                
                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<AddConnectionViewModel>();
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
        _mainWindow.Activate();
        
        // Auto-mount configured drives
        var mountManager = Services.GetRequiredService<IMountManager>();
        await mountManager.AutoMountAsync();
    }

    public static void ShowWindow()
    {
        var window = MainWindowInstance;
        if (window != null)
        {
            window.Activate();
        }
    }

    public static async Task ExitApplicationAsync()
    {
        // Unmount all drives gracefully
        var mountManager = Services.GetRequiredService<IMountManager>();
        await mountManager.UnmountAllAsync();
        
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        
        Environment.Exit(0);
    }
}
