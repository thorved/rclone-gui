using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.Models;
using RcloneGui.ViewModels;

namespace RcloneGui.Views.ConnectionType.Ftp;

public sealed partial class FtpSettingsDialog : ContentDialog
{
    public AddFtpConnectionViewModel ViewModel { get; }
    private FtpConnection? _connection;

    public FtpSettingsDialog(FtpConnection connection)
    {
        // IMPORTANT: Initialize ViewModel BEFORE InitializeComponent for x:Bind to work
        ViewModel = App.Services.GetRequiredService<AddFtpConnectionViewModel>();
        
        this.InitializeComponent();
        
        _connection = connection;
        ViewModel.LoadConnection(connection);
        
        // Set up auth panel visibility based on anonymous state
        if (AuthPanel != null)
        {
            AuthPanel.Visibility = connection.IsAnonymous ? Visibility.Collapsed : Visibility.Visible;
        }
        
        Bindings.Update();
    }

    private void AnonymousCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (AuthPanel != null)
        {
            AuthPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void AnonymousCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (AuthPanel != null)
        {
            AuthPanel.Visibility = Visibility.Visible;
        }
    }

    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            return await ViewModel.SaveAsync();
        }
        
        return false;
    }
}
