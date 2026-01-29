using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.ViewModels;
using RcloneGui.Views.ConnectionType.Sftp;

namespace RcloneGui.Views;

public sealed partial class DrivesView : Page
{
    public MainViewModel ViewModel { get; }

    public DrivesView()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Always refresh drives when navigating to this page
        await ViewModel.RefreshDrivesCommand.ExecuteAsync(null);
    }

    private void AddConnection_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to Add Connection page
        App.MainWindowInstance?.NavigateToAddConnection();
    }

    private async void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // Get DriveItemViewModel directly from the Button's Tag
        if (sender is Button button && button.Tag is DriveItemViewModel driveVm)
        {
            System.Diagnostics.Debug.WriteLine($"Settings clicked for: {driveVm.Connection.Name}, Id: {driveVm.Connection.Id}");
            
            var dialog = new SftpSettingsDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.LoadConnection(driveVm.Connection);
            
            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                // Refresh drives after saving
                await ViewModel.RefreshDrivesCommand.ExecuteAsync(null);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Settings clicked but could not get DriveItemViewModel from Tag");
        }
    }

    private void DuplicateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is DriveItemViewModel driveVm)
        {
            driveVm.DuplicateCommand.Execute(null);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is DriveItemViewModel driveVm)
        {
            driveVm.DeleteCommand.Execute(null);
        }
    }
}
