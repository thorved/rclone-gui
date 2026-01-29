using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace RcloneGui.Core.Services;

/// <summary>
/// Implementation of notification service using Windows App SDK notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IConfigManager _configManager;
    private bool _isInitialized;

    public NotificationService(IConfigManager configManager)
    {
        _configManager = configManager;
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            // For unpackaged apps, we need to register the notification manager
            AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            AppNotificationManager.Default.Register();
            _isInitialized = true;
        }
        catch
        {
            // Notifications may not be available
            _isInitialized = false;
        }
    }

    private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // Handle notification click - bring app to foreground
        // This will be handled by the App class
    }

    private bool ShouldShowNotification()
    {
        return _isInitialized && (_configManager.Settings?.ShowNotifications ?? true);
    }

    public void ShowMountNotification(string connectionName, string driveLetter)
    {
        if (!ShouldShowNotification()) return;

        try
        {
            var notification = new AppNotificationBuilder()
                .AddText("Drive Mounted")
                .AddText($"{connectionName} is now available as {driveLetter}:\\")
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
        catch
        {
            // Ignore notification errors
        }
    }

    public void ShowUnmountNotification(string connectionName, string driveLetter)
    {
        if (!ShouldShowNotification()) return;

        try
        {
            var notification = new AppNotificationBuilder()
                .AddText("Drive Unmounted")
                .AddText($"{connectionName} ({driveLetter}:\\) has been disconnected")
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
        catch
        {
            // Ignore notification errors
        }
    }

    public void ShowMountErrorNotification(string connectionName, string errorMessage)
    {
        if (!ShouldShowNotification()) return;

        try
        {
            var notification = new AppNotificationBuilder()
                .AddText("Mount Failed")
                .AddText($"Failed to mount {connectionName}: {errorMessage}")
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
        catch
        {
            // Ignore notification errors
        }
    }

    public void ShowNotification(string title, string message)
    {
        if (!ShouldShowNotification()) return;

        try
        {
            var notification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
        catch
        {
            // Ignore notification errors
        }
    }
}
