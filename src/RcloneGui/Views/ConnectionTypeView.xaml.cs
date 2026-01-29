using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RcloneGui.Views.ConnectionType.Sftp;

namespace RcloneGui.Views;

/// <summary>
/// Page for selecting the type of connection to add.
/// </summary>
public sealed partial class ConnectionTypeView : Page
{
    public ConnectionTypeView()
    {
        this.InitializeComponent();
    }

    private void SftpCard_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to SftpConnectionView for new SFTP connection
        App.MainWindowInstance?.NavigateToSftpConnection(null);
    }
}
