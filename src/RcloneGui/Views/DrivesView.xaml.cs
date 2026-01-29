using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RcloneGui.ViewModels;

namespace RcloneGui.Views;

public sealed partial class DrivesView : Page
{
    public MainViewModel ViewModel { get; }
    
    // Store the current drive item for menu flyout actions
    private DriveItemViewModel? _currentFlyoutItem;

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

    private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        // Store the DriveItemViewModel from the button's Tag when flyout opens
        if (sender is Button button && button.Tag is DriveItemViewModel driveVm)
        {
            _currentFlyoutItem = driveVm;
            System.Diagnostics.Debug.WriteLine($"MoreOptions clicked, stored: {driveVm.Connection.Name}");
        }
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFlyoutItem != null)
        {
            System.Diagnostics.Debug.WriteLine($"Edit clicked for: {_currentFlyoutItem.Connection.Name}");
            App.MainWindowInstance?.NavigateToAddConnection(_currentFlyoutItem.Connection);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Edit clicked but _currentFlyoutItem is null");
        }
    }

    private void DuplicateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFlyoutItem != null)
        {
            _currentFlyoutItem.DuplicateCommand.Execute(null);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFlyoutItem != null)
        {
            _currentFlyoutItem.DeleteCommand.Execute(null);
        }
    }
}
