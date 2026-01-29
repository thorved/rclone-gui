using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.Features.Ftp.Models;
using RcloneGui.Features.Ftp.ViewModels;

namespace RcloneGui.Features.Ftp.Views;

public sealed partial class FtpConnectionView : Page
{
    public AddFtpConnectionViewModel ViewModel { get; }

    public FtpConnectionView()
    {
        // IMPORTANT: Initialize ViewModel BEFORE InitializeComponent for x:Bind to work
        ViewModel = App.Services.GetRequiredService<AddFtpConnectionViewModel>();
        
        this.InitializeComponent();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // This page is only for adding new connections
        // Editing is done via FtpSettingsDialog
        ViewModel.RefreshAvailableDriveLetters();
        ViewModel.ResetForm();
        
        // Reset anonymous checkbox and auth panel visibility
        if (AnonymousCheckBox != null)
        {
            AnonymousCheckBox.IsChecked = false;
        }
        if (AuthPanel != null)
        {
            AuthPanel.Visibility = Visibility.Visible;
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

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var success = await ViewModel.SaveAsync();
        if (success)
        {
            // Navigate back to drives view using MainWindow
            App.MainWindowInstance?.NavigateToDrives();
        }
    }
}
