using System.Windows;
using System.Windows.Controls;

namespace Explorer.UI.Controls.ExplorerNavigation;

[TemplatePart(Name = PartTreeView, Type = typeof(TreeView))]
public class ExplorerNavigation : Control
{
    public const string PartTreeView = "PART_TreeView";

    private TreeView? _treeView;

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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_treeView != null)
            _treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;

        _treeView = GetTemplateChild(PartTreeView) as TreeView;

        if (_treeView != null)
            _treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = e.NewValue;
    }
}