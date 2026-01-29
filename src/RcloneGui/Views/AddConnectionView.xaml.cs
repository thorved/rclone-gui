using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.ViewModels;
using Windows.Storage.Pickers;

namespace RcloneGui.Views;

public sealed partial class AddConnectionView : Page
{
    public AddConnectionViewModel ViewModel { get; }

    public AddConnectionView()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AddConnectionViewModel>();
        DataContext = ViewModel;
        
        // Initialize available drive letters
        ViewModel.RefreshAvailableDriveLetters();
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
                ViewModel.AuthType = Models.AuthenticationType.Password;
            }
            else if (tag == "Key")
            {
                PasswordPanel.Visibility = Visibility.Collapsed;
                KeyPanel.Visibility = Visibility.Visible;
                ViewModel.AuthType = Models.AuthenticationType.KeyFile;
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
}
