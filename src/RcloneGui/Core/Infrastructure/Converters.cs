using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using RcloneGui.Core.Models;

namespace RcloneGui.Core.Infrastructure;

/// <summary>
/// Converts a boolean to visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}

/// <summary>
/// Inverts a boolean value.
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return !b;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return !b;
        }
        return false;
    }
}

/// <summary>
/// Inverts bool to visibility.
/// </summary>
public class InvertBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}

/// <summary>
/// Converts string to bool (true if not null/empty).
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts string to visibility.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts MountStatus.Mounted to visibility.
/// </summary>
public class MountedToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MountStatus status)
        {
            return status == MountStatus.Mounted ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts MountStatus to button text (Mount/Unmount).
/// </summary>
public class MountButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MountStatus status)
        {
            return status switch
            {
                MountStatus.Mounted => "Unmount",
                MountStatus.Mounting => "Mounting...",
                MountStatus.Unmounting => "Unmounting...",
                _ => "Mount"
            };
        }
        return "Mount";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts bool to status color (green/red).
/// </summary>
public class BoolToStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return new SolidColorBrush(b ? Colors.Green : Colors.Red);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts WinFsp installed bool to status text.
/// </summary>
public class WinFspStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool installed)
        {
            return installed ? "WinFsp Installed" : "WinFsp Not Installed";
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts test success to InfoBar title.
/// </summary>
public class TestResultTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool success)
        {
            return success ? "Success" : "Failed";
        }
        return "Test Result";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts test success to InfoBar severity.
/// </summary>
public class TestResultSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool success)
        {
            return success 
                ? Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success 
                : Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
        }
        return Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverse bool to visibility (for binding compatibility).
/// Alias for InvertBoolToVisibilityConverter.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}

/// <summary>
/// Converts AuthenticationType to visibility for Password/KeyFile panels.
/// Parameter should be "Password" or "KeyFile".
/// </summary>
public class AuthTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Features.Sftp.Models.AuthenticationType authType && parameter is string targetType2)
        {
            bool isMatch = authType.ToString() == targetType2;
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts MountStatus to a color brush.
/// </summary>
public class MountStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MountStatus status)
        {
            return status switch
            {
                MountStatus.Mounted => new SolidColorBrush(Colors.Green),
                MountStatus.Mounting => new SolidColorBrush(Colors.Orange),
                MountStatus.Unmounting => new SolidColorBrush(Colors.Orange),
                MountStatus.Error => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts MountStatus to display text.
/// </summary>
public class MountStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MountStatus status)
        {
            return status switch
            {
                MountStatus.Mounted => "Connected",
                MountStatus.Mounting => "Connecting...",
                MountStatus.Unmounting => "Disconnecting...",
                MountStatus.Error => "Error",
                _ => "Disconnected"
            };
        }
        return "Disconnected";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value (alias for InvertBoolConverter).
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return !b;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return !b;
        }
        return false;
    }
}

/// <summary>
/// Converts bool to InfoBar severity (Success/Error).
/// </summary>
public class BoolToSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool success)
        {
            return success 
                ? Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success 
                : Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
        }
        return Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts bool to test title text.
/// </summary>
public class BoolToTestTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool success)
        {
            return success ? "Connection Successful" : "Connection Failed";
        }
        return "Test Result";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
