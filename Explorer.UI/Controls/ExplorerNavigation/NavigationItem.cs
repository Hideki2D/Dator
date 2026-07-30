using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Explorer.UI.Controls;

public class NavigationItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private ImageSource? _icon;
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isEnabled = true;
    private bool _childrenLoaded;

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public ImageSource? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetField(ref _isExpanded, value) && value)
                _ = EnsureChildrenLoadedAsync();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    public object? Tag { get; set; }

    /// <summary>
    /// Explorer подставляет сюда логику чтения файловой системы —
    /// этот проект (Explorer.UI) про System.IO ничего не знает.
    /// </summary>
    public Func<NavigationItem, Task>? LoadChildren { get; set; }

    public ObservableCollection<NavigationItem> Items { get; } = new();

    /// <summary>
    /// Подгружает дочерние узлы один раз (если ещё не загружены).
    /// Можно вызывать вручную — не обязательно через разворачивание в UI.
    /// </summary>
    public async Task EnsureChildrenLoadedAsync()
    {
        if (_childrenLoaded)
            return;

        _childrenLoaded = true;

        if (LoadChildren != null)
            await LoadChildren(this);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}