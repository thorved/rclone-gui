using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.Features.Dashboard.ViewModels;
using RcloneGui.Features.Sftp.Views;
using RcloneGui.Features.Ftp.Views;
using System.Linq;

namespace RcloneGui.Features.Dashboard.Views;

/// <summary>
/// Page for selecting the type of connection to add.
/// </summary>
public sealed partial class ConnectionTypeView : Page
{
    public ConnectionTypeViewModel ViewModel { get; } = new();

    public ConnectionTypeView()
    {
        this.InitializeComponent();
        DataContext = ViewModel;
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.FilterConnectionTypes(sender.Text);
            UpdateNoResultsVisibility();
        }
    }

    private void UpdateNoResultsVisibility()
    {
        bool hasResults = ViewModel.Categories.Any(cat => cat.Items.Count > 0);
        NoResultsPanel.Visibility = hasResults ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ConnectionTypeCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ConnectionTypeItem item)
        {
            switch (item.Id)
            {
                case "sftp":
                    App.MainWindowInstance?.NavigateToSftpConnection(null);
                    break;
                case "ftp":
                    App.MainWindowInstance?.NavigateToFtpConnection(null);
                    break;
            }
        }
    }
}
