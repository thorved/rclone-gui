using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RcloneGui.Features.Dashboard.ViewModels;

public class ConnectionTypeItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\uE753";
    public string IconBackgroundColor { get; set; } = "#797979";
    public string IconForegroundColor { get; set; } = "#FFFFFF";
    public bool IsEnabled { get; set; } = true;
    public string StatusText { get; set; } = string.Empty;
    public bool ShowStatus => !string.IsNullOrEmpty(StatusText);
}

public class ConnectionTypeCategory
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "\uE753";
    public ObservableCollection<ConnectionTypeItem> Items { get; set; } = new();
    public string ItemCountText => $"({Items.Count})";
}

public class ConnectionTypeViewModel : INotifyPropertyChanged
{
    private readonly List<ConnectionTypeItem> _allConnectionTypes;
    private ObservableCollection<ConnectionTypeCategory> _categories = new();

    public ObservableCollection<ConnectionTypeCategory> Categories
    {
        get => _categories;
        private set
        {
            _categories = value;
            OnPropertyChanged(nameof(Categories));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ConnectionTypeViewModel()
    {
        _allConnectionTypes = GetAllConnectionTypes();
        LoadCategories();
    }

    private void LoadCategories()
    {
        Categories.Clear();
        
        var grouped = _allConnectionTypes
            .GroupBy(ct => ct.Category)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            Categories.Add(new ConnectionTypeCategory
            {
                CategoryName = group.Key,
                CategoryIcon = GetCategoryIcon(group.Key),
                Items = new ObservableCollection<ConnectionTypeItem>(group)
            });
        }
    }

    public void FilterConnectionTypes(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            LoadCategories();
            return;
        }

        Categories.Clear();
        
        var filtered = _allConnectionTypes
            .Where(ct => ct.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        ct.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .GroupBy(ct => ct.Category)
            .OrderBy(g => g.Key);

        foreach (var group in filtered)
        {
            Categories.Add(new ConnectionTypeCategory
            {
                CategoryName = group.Key,
                CategoryIcon = GetCategoryIcon(group.Key),
                Items = new ObservableCollection<ConnectionTypeItem>(group)
            });
        }
    }

    private static string GetCategoryIcon(string category) => category switch
    {
        "Cloud Storage" => "\uE753",
        "File Transfer" => "\uE8B7",
        "Object Storage" => "\uE7B8",
        "Other" => "\uE774",
        _ => "\uE753"
    };

    private static List<ConnectionTypeItem> GetAllConnectionTypes()
    {
        const string AccentColor = "#0078D4";
        const string DisabledColor = "#797979";
        const string WhiteColor = "#FFFFFF";
        const string GrayColor = "#A0A0A0";

        return new List<ConnectionTypeItem>
        {
            // File Transfer
            new ConnectionTypeItem
            {
                Id = "sftp",
                Name = "SFTP",
                Description = "Secure File Transfer Protocol",
                Category = "File Transfer",
                IconGlyph = "\uE8B7",
                IconBackgroundColor = AccentColor,
                IconForegroundColor = WhiteColor,
                IsEnabled = true
            },
            new ConnectionTypeItem
            {
                Id = "ftp",
                Name = "FTP",
                Description = "File Transfer Protocol",
                Category = "File Transfer",
                IconGlyph = "\uE8B7",
                IconBackgroundColor = AccentColor,
                IconForegroundColor = WhiteColor,
                IsEnabled = true
            },

            // Cloud Storage
            new ConnectionTypeItem
            {
                Id = "googledrive",
                Name = "Google Drive",
                Description = "Google Cloud Storage",
                Category = "Cloud Storage",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "onedrive",
                Name = "OneDrive",
                Description = "Microsoft OneDrive",
                Category = "Cloud Storage",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "dropbox",
                Name = "Dropbox",
                Description = "Dropbox Cloud Storage",
                Category = "Cloud Storage",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "box",
                Name = "Box",
                Description = "Box Cloud Storage",
                Category = "Cloud Storage",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "pcloud",
                Name = "pCloud",
                Description = "pCloud Storage",
                Category = "Cloud Storage",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },

            // Object Storage
            new ConnectionTypeItem
            {
                Id = "s3",
                Name = "Amazon S3",
                Description = "AWS S3 Object Storage",
                Category = "Object Storage",
                IconGlyph = "\uE7B8",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "wasabi",
                Name = "Wasabi",
                Description = "Wasabi Hot Cloud Storage",
                Category = "Object Storage",
                IconGlyph = "\uE7B8",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "backblaze",
                Name = "Backblaze B2",
                Description = "Backblaze B2 Cloud Storage",
                Category = "Object Storage",
                IconGlyph = "\uE7B8",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },

            // Other
            new ConnectionTypeItem
            {
                Id = "webdav",
                Name = "WebDAV",
                Description = "WebDAV Protocol",
                Category = "Other",
                IconGlyph = "\uE774",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "nextcloud",
                Name = "Nextcloud",
                Description = "Nextcloud Storage",
                Category = "Other",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            },
            new ConnectionTypeItem
            {
                Id = "owncloud",
                Name = "ownCloud",
                Description = "ownCloud Storage",
                Category = "Other",
                IconGlyph = "\uE753",
                IconBackgroundColor = DisabledColor,
                IconForegroundColor = GrayColor,
                IsEnabled = false,
                StatusText = "Soon"
            }
        };
    }
}
