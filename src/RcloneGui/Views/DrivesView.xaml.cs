using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.ViewModels;

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

    private void AddConnection_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to Add Connection page
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(AddConnectionView));
        }
    }
}
