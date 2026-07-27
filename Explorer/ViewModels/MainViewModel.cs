using Explorer.Models;
using Explorer.Services;
using Explorer.UI.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Explorer.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<NavigationItem> NavigationRoots { get; } = new();

    public ObservableCollection<FileSystemItem> Items { get; } = new();

    private NavigationItem? _selectedNavigationItem;
    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetField(ref _selectedNavigationItem, value) && value?.Tag is string path)
                LoadFolder(path);
        }
    }

    private string _currentPath = string.Empty;
    public string CurrentPath
    {
        get => _currentPath;
        private set => SetField(ref _currentPath, value);
    }

    public void LoadDrives()
    {
        NavigationRoots.Clear();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
                continue;

            NavigationRoots.Add(CreateFolderNode(drive.RootDirectory.FullName, drive.Name.TrimEnd('\\')));
        }
    }

    private NavigationItem CreateFolderNode(string path, string? displayName = null)
    {
        var node = new NavigationItem
        {
            Title = displayName ?? Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)),
            Icon = ShellIconProvider.GetIcon(path),
            Tag = path,
            LoadChildren = LoadChildNodesAsync
        };

        // заглушка, чтобы у узла сразу была стрелка "развернуть",
        // не сканируя диск заранее
        node.Items.Add(new NavigationItem { Title = "Загрузка..." });

        return node;
    }

    private Task LoadChildNodesAsync(NavigationItem node)
    {
        if (node.Tag is not string path)
            return Task.CompletedTask;

        return Task.Run(() =>
        {
            var children = SafeEnumerateDirectories(path)
                .Select(dir => CreateFolderNode(dir))
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                node.Items.Clear();

                foreach (var child in children)
                    node.Items.Add(child);
            });
        });
    }

    public void LoadFolder(string path)
    {
        Items.Clear();
        CurrentPath = path;

        foreach (var dir in SafeEnumerateDirectories(path))
        {
            var item = new FileSystemItem
            {
                Name = Path.GetFileName(dir),
                FullPath = dir,
                Icon = ShellIconProvider.GetIcon(dir),
                IsDirectory = true
            };

            item.OpenCommand = new RelayCommand(() => LoadFolder(item.FullPath));

            Items.Add(item);
        }

        foreach (var file in SafeEnumerateFiles(path))
        {
            Items.Add(new FileSystemItem
            {
                Name = Path.GetFileName(file),
                FullPath = file,
                Icon = ShellIconProvider.GetIcon(file)
            });
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string path)
    {
        try
        {
            return Directory.EnumerateDirectories(path).ToList();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return Enumerable.Empty<string>();
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string path)
    {
        try
        {
            return Directory.EnumerateFiles(path).ToList();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return Enumerable.Empty<string>();
        }
    }
}