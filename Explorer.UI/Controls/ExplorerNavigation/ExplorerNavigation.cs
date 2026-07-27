using System.Windows;
using System.Windows.Controls;

namespace Explorer.UI.Controls.ExplorerNavigation;

public class ExplorerNavigation : Control
{
    static ExplorerNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExplorerNavigation),
            new FrameworkPropertyMetadata(typeof(ExplorerNavigation)));
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(ExplorerNavigation),
            new PropertyMetadata(null));

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ExplorerNavigation),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
}