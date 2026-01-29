namespace RcloneGui.Core.Services;

/// <summary>
/// Service for showing toast notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Show a notification when a drive is mounted.
    /// </summary>
    void ShowMountNotification(string connectionName, string driveLetter);

    /// <summary>
    /// Show a notification when a drive is unmounted.
    /// </summary>
    void ShowUnmountNotification(string connectionName, string driveLetter);

    /// <summary>
    /// Show a notification when a mount fails.
    /// </summary>
    void ShowMountErrorNotification(string connectionName, string errorMessage);

    /// <summary>
    /// Show a general notification.
    /// </summary>
    void ShowNotification(string title, string message);
}
