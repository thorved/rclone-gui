using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.Models;
using RcloneGui.ViewModels;
using Windows.Storage.Pickers;

namespace RcloneGui.Views.ConnectionType.Sftp;

public sealed partial class SftpConnectionView : Page
{
    public AddConnectionViewModel ViewModel { get; }

    public SftpConnectionView()
    {
        // IMPORTANT: Initialize ViewModel BEFORE InitializeComponent for x:Bind to work
        ViewModel = App.Services.GetRequiredService<AddConnectionViewModel>();
        
        this.InitializeComponent();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // This page is only for adding new connections
        // Editing is done via SftpSettingsDialog
        ViewModel.RefreshAvailableDriveLetters();
        ViewModel.ResetForm();
        
        // Reset auth type UI
        PasswordRadio.IsChecked = true;
        PasswordPanel.Visibility = Visibility.Visible;
        KeyPanel.Visibility = Visibility.Collapsed;
        
        Bindings.Update();
    }

    private void AuthType_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.IsChecked == true)
        {
            var tag = rb.Tag?.ToString();
            // Check if controls are initialized
            if (PasswordPanel == null || KeyPanel == null) return;
            
            if (tag == "Password")
            {
                PasswordPanel.Visibility = Visibility.Visible;
                KeyPanel.Visibility = Visibility.Collapsed;
                ViewModel.AuthType = AuthenticationType.Password;
            }
            else if (tag == "Key")
            {
                PasswordPanel.Visibility = Visibility.Collapsed;
                KeyPanel.Visibility = Visibility.Visible;
                ViewModel.AuthType = AuthenticationType.KeyFile;
            }
        }
    }

    private async void BrowseKeyFile_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        
        // Get the window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ViewModel.KeyFilePath = file.Path;
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
