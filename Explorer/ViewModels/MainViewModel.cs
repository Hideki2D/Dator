using Explorer.Models;
using Explorer.Services;
using Explorer.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Explorer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private bool _isNavigatingProgrammatically;
    private bool _isNavigatingHistory;

    private readonly List<string> _history = new();
    private int _historyIndex = -1;

    public ObservableCollection<NavigationItem> NavigationRoots { get; } = new();

    public ObservableCollection<FileSystemItem> Items { get; } = new();

    public event Action<string>? OpenWithRequested;

    public ICommand BackCommand { get; }
    public ICommand ForwardCommand { get; }
    public ICommand UpCommand { get; }
    public ICommand RefreshCommand { get; }

    public MainViewModel()
    {
        BackCommand = new RelayCommand(GoBack, () => CanGoBack);
        ForwardCommand = new RelayCommand(GoForward, () => CanGoForward);
        UpCommand = new RelayCommand(GoUp, () => CanGoUp);
        RefreshCommand = new RelayCommand(Refresh, () => !string.IsNullOrEmpty(CurrentPath));
    }

    private NavigationItem? _selectedNavigationItem;
    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetField(ref _selectedNavigationItem, value) &&
                !_isNavigatingProgrammatically &&
                value?.Tag is string path)
            {
                NavigateToInternal(path, pushHistory: true);
            }
        }
    }

    private string _currentPath = string.Empty;
    public string CurrentPath
    {
        get => _currentPath;
        private set
        {
            if (SetField(ref _currentPath, value))
                OnPropertyChanged(nameof(CanGoUp));
        }
    }

    public bool CanGoBack => _historyIndex > 0;

    public bool CanGoForward => _historyIndex >= 0 && _historyIndex < _history.Count - 1;

    public bool CanGoUp => !string.IsNullOrEmpty(CurrentPath) && Directory.GetParent(CurrentPath) != null;

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
            Tag = Path.TrimEndingDirectorySeparator(path),
            LoadChildren = LoadChildNodesAsync
        };

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
            var dirItem = new FileSystemItem
            {
                Name = Path.GetFileName(dir),
                FullPath = dir,
                Icon = ShellIconProvider.GetIcon(dir),
                IsDirectory = true
            };

            dirItem.OpenCommand = new RelayCommand(() => NavigateTo(dirItem.FullPath));

            Items.Add(dirItem);
        }

        foreach (var file in SafeEnumerateFiles(path))
        {
            var fileItem = new FileSystemItem
            {
                Name = Path.GetFileName(file),
                FullPath = file,
                Icon = ShellIconProvider.GetIcon(file)
            };

            fileItem.OpenCommand = new RelayCommand(() => FileAssociationService.OpenWithDefault(fileItem.FullPath));
            fileItem.OpenWithCommand = new RelayCommand(() => OpenWithRequested?.Invoke(fileItem.FullPath));

            Items.Add(fileItem);
        }
    }

    /// <summary>
    /// Переход в папку из любого места (список, "вверх" и т.д.) — добавляет запись в историю.
    /// </summary>
    public void NavigateTo(string path) => NavigateToInternal(path, pushHistory: true);

    private void NavigateToInternal(string path, bool pushHistory)
    {
        LoadFolder(path);
        _ = SyncNavigationSelectionAsync(path);

        if (pushHistory && !_isNavigatingHistory)
        {
            if (_historyIndex < _history.Count - 1)
                _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);

            if (_historyIndex < 0 || !string.Equals(_history[_historyIndex], path, StringComparison.OrdinalIgnoreCase))
            {
                _history.Add(path);
                _historyIndex++;
            }

            RaiseHistoryChanged();
        }
    }

    public void GoBack()
    {
        if (!CanGoBack)
            return;

        _historyIndex--;
        NavigateHistoryEntry();
    }

    public void GoForward()
    {
        if (!CanGoForward)
            return;

        _historyIndex++;
        NavigateHistoryEntry();
    }

    private void NavigateHistoryEntry()
    {
        _isNavigatingHistory = true;
        NavigateToInternal(_history[_historyIndex], pushHistory: false);
        _isNavigatingHistory = false;

        RaiseHistoryChanged();
    }

    public void GoUp()
    {
        var parent = Directory.GetParent(CurrentPath);

        if (parent != null)
            NavigateTo(parent.FullName);
    }

    public void Refresh()
    {
        if (!string.IsNullOrEmpty(CurrentPath))
            LoadFolder(CurrentPath);
    }

    private void RaiseHistoryChanged()
    {
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    private async Task SyncNavigationSelectionAsync(string path)
    {
        var node = await FindOrExpandNodeAsync(path);

        if (node is null)
            return;

        _isNavigatingProgrammatically = true;

        SelectedNavigationItem = node;
        node.IsSelected = true;

        _isNavigatingProgrammatically = false;
    }

    private async Task<NavigationItem?> FindOrExpandNodeAsync(string path)
    {
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var root = Path.GetPathRoot(fullPath);

        if (string.IsNullOrEmpty(root))
            return null;

        var rootTrimmed = Path.TrimEndingDirectorySeparator(root);

        var current = NavigationRoots.FirstOrDefault(n =>
            string.Equals(n.Tag as string, rootTrimmed, StringComparison.OrdinalIgnoreCase));

        if (current is null)
            return null;

        if (string.Equals(fullPath, rootTrimmed, StringComparison.OrdinalIgnoreCase))
            return current;

        var relative = fullPath[rootTrimmed.Length..].Trim(Path.DirectorySeparatorChar);
        var segments = relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var accumulatedPath = rootTrimmed;

        foreach (var segment in segments)
        {
            accumulatedPath = Path.TrimEndingDirectorySeparator(Path.Combine(accumulatedPath, segment));

            await current.EnsureChildrenLoadedAsync();

            var next = current.Items.FirstOrDefault(n =>
                string.Equals(n.Tag as string, accumulatedPath, StringComparison.OrdinalIgnoreCase));

            if (next is null)
                return current;

            current.IsExpanded = true;
            current = next;
        }

        return current;
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