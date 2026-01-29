using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.Features.Sftp.Models;
using RcloneGui.Features.Ftp.Models;
using RcloneGui.Features.Dashboard.ViewModels;
using RcloneGui.Features.Sftp.Views;
using RcloneGui.Features.Ftp.Views;

namespace RcloneGui.Features.Dashboard.Views;

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
            var connectionName = driveVm.Name;
            var connectionId = driveVm.Connection is SftpConnection sftp ? sftp.Id : (driveVm.Connection as FtpConnection)?.Id ?? "Unknown";
            System.Diagnostics.Debug.WriteLine($"Settings clicked for: {connectionName}, Id: {connectionId}");
            
            bool? success = null;
            
            if (driveVm.IsSftp && driveVm.SftpConnection is SftpConnection sftpConn)
            {
                var dialog = new SftpSettingsDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.LoadConnection(sftpConn);
                var result = await dialog.ShowAsync();
                success = result == ContentDialogResult.Primary;
            }
            else if (driveVm.IsFtp && driveVm.FtpConnection is FtpConnection ftpConn)
            {
                var dialog = new FtpSettingsDialog(ftpConn);
                dialog.XamlRoot = this.XamlRoot;
                success = await dialog.ShowAsync();
            }
            
            if (success == true)
            {
                // Refresh drives after saving
                await ViewModel.RefreshDrivesAsync();
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
