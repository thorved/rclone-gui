using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.Core.Models;
using RcloneGui.Core.Services;
using RcloneGui.Features.Settings.ViewModels;
using System.Linq;

namespace RcloneGui.Features.Settings.Views;

/// <summary>
/// Page for configuring global rclone performance and VFS settings.
/// </summary>
public sealed partial class RclonePerformanceSettingsPage : Page
{
    public RclonePerformanceSettingsViewModel ViewModel { get; }

    public RclonePerformanceSettingsPage()
    {
        this.InitializeComponent();
        
        var configManager = App.Services.GetService(typeof(IConfigManager)) as IConfigManager 
            ?? throw new InvalidOperationException("ConfigManager not available");
        
        ViewModel = new RclonePerformanceSettingsViewModel(configManager);
        this.DataContext = ViewModel;
        
        // Initialize ComboBox items
        CacheModeCombo.ItemsSource = Enum.GetValues<VfsCacheMode>();
        ProfileComboBox.ItemsSource = Enum.GetValues<VfsPerformanceProfile>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = ViewModel.InitializeAsync();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
        
        // Show success notification
        var dialog = new ContentDialog
        {
            Title = "Settings Saved",
            Content = "Your rclone performance settings have been saved.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        
        await dialog.ShowAsync();
    }

    private async void ResetToDefaults_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Reset to Defaults",
            Content = "Are you sure you want to reset all performance settings to their default values?",
            PrimaryButtonText = "Reset",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.ResetToDefaults();
        }
    }

    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is VfsPerformanceProfile profile)
        {
            ViewModel.ApplyPerformanceProfile(profile);
            
            // Show info about the selected profile
            ProfileInfoBar.Message = $"Applied {profile.GetDisplayName()}: {profile.GetDescription()}";
            ProfileInfoBar.Severity = InfoBarSeverity.Informational;
            ProfileInfoBar.IsOpen = true;
        }
    }
}

/// <summary>
/// Converter to get display name from VfsPerformanceProfile.
/// </summary>
public class ProfileDisplayNameConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is VfsPerformanceProfile profile)
        {
            return profile.GetDisplayName();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to get description from VfsPerformanceProfile.
/// </summary>
public class ProfileDescriptionConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is VfsPerformanceProfile profile)
        {
            return profile.GetDescription();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
