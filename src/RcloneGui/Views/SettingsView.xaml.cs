using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.ViewModels;
using Windows.Storage.Pickers;

namespace RcloneGui.Views;

public sealed partial class SettingsView : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsView()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        DataContext = ViewModel;
        
        // Load settings
        _ = ViewModel.LoadSettingsAsync();
    }

    private async void BrowseRclone_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".exe");
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ViewModel.CustomRclonePath = file.Path;
        }
    }

    private async void BrowseCacheDir_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ViewModel.CacheDirectory = folder.Path;
        }
    }

    private async void ExportConfig_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("JSON Configuration", new[] { ".json" });
        picker.SuggestedFileName = $"rclonegui_config_{DateTime.Now:yyyyMMdd_HHmmss}";
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            await ViewModel.ExportConfigToPathAsync(file.Path);
        }
    }

    private async void ImportConfig_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".json");
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            await ViewModel.ImportConfigFromPathAsync(file.Path);
        }
    }
}
