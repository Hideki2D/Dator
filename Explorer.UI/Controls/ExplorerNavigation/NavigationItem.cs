using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Explorer.UI.Controls;

/// <summary>
/// Represents a single item displayed in the NavigationView.
/// </summary>
public class NavigationItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private ImageSource? _icon;
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isEnabled = true;

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
        set => SetField(ref _isExpanded, value);
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

    public ObservableCollection<NavigationItem> Items { get; } = new();

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