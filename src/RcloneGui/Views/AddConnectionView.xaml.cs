using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.Models;
using RcloneGui.ViewModels;
using Windows.Storage.Pickers;

namespace RcloneGui.Views;

public sealed partial class AddConnectionView : Page
{
    public AddConnectionViewModel ViewModel { get; }

    public AddConnectionView()
    {
        // IMPORTANT: Initialize ViewModel BEFORE InitializeComponent for x:Bind to work
        ViewModel = App.Services.GetRequiredService<AddConnectionViewModel>();
        
        this.InitializeComponent();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"AddConnectionView.OnNavigatedTo - Parameter type: {e.Parameter?.GetType().Name ?? "null"}");
            
            // Refresh available drive letters first
            ViewModel.RefreshAvailableDriveLetters();
            
            // If editing, load the connection data
            if (e.Parameter is SftpConnection connection)
            {
                System.Diagnostics.Debug.WriteLine($"Editing connection: {connection.Name}, Host: {connection.Host}");
                
                // Load data into ViewModel (this handles everything including _editingConnectionId)
                ViewModel.LoadConnection(connection);
                
                // Update page title
                PageTitle.Text = "Edit SFTP Connection";
                
                // Force control values directly as fallback
                NameBox.Text = connection.Name;
                HostBox.Text = connection.Host;
                PortBox.Value = connection.Port;
                UsernameBox.Text = connection.Username;
                
                // Update RadioButton selection for auth type
                AuthTypeRadio.SelectedIndex = connection.AuthType == AuthenticationType.Password ? 0 : 1;
                
                // Update UI for auth type panels
                if (connection.AuthType == AuthenticationType.Password)
                {
                    PasswordPanel.Visibility = Visibility.Visible;
                    KeyPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PasswordPanel.Visibility = Visibility.Collapsed;
                    KeyPanel.Visibility = Visibility.Visible;
                }
                
                System.Diagnostics.Debug.WriteLine($"Controls updated - NameBox.Text: {NameBox.Text}");
            }
            else
            {
                // Adding new connection - reset form
                ViewModel.ResetForm();
                PageTitle.Text = "Add SFTP Connection";
                AuthTypeRadio.SelectedIndex = 0;
                PasswordPanel.Visibility = Visibility.Visible;
                KeyPanel.Visibility = Visibility.Collapsed;
            }
            
            // Force x:Bind to re-evaluate all bindings with the current ViewModel state
            Bindings.Update();
            
            System.Diagnostics.Debug.WriteLine($"After Bindings.Update - ViewModel.Name: {ViewModel.Name}, NameBox.Text: {NameBox.Text}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnNavigatedTo: {ex}");
        }
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
