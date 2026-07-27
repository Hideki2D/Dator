using System.Windows;
using System.Windows.Controls;

namespace Explorer.UI.Controls.Layout;

/// <summary>
/// Root layout used by shell-style applications.
/// </summary>
[TemplatePart(Name = PartToolbar, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartAddressBar, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartNavigation, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartMainContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartStatusBar, Type = typeof(ContentPresenter))]
public class ShellLayout : Control
{
    #region Template Parts

    public const string PartToolbar = "PART_Toolbar";
    public const string PartAddressBar = "PART_AddressBar";
    public const string PartNavigation = "PART_Navigation";
    public const string PartMainContent = "PART_MainContent";
    public const string PartStatusBar = "PART_StatusBar";

    #endregion

    static ShellLayout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ShellLayout),
            new FrameworkPropertyMetadata(typeof(ShellLayout)));
    }

    #region ToolbarContent

    public static readonly DependencyProperty ToolbarContentProperty =
        DependencyProperty.Register(
            nameof(ToolbarContent),
            typeof(object),
            typeof(ShellLayout),
            new PropertyMetadata(null));

    public object? ToolbarContent
    {
        get => GetValue(ToolbarContentProperty);
        set => SetValue(ToolbarContentProperty, value);
    }

    #endregion

    #region AddressBarContent

    public static readonly DependencyProperty AddressBarContentProperty =
        DependencyProperty.Register(
            nameof(AddressBarContent),
            typeof(object),
            typeof(ShellLayout),
            new PropertyMetadata(null));

    public object? AddressBarContent
    {
        get => GetValue(AddressBarContentProperty);
        set => SetValue(AddressBarContentProperty, value);
    }

    #endregion

    #region NavigationContent

    public static readonly DependencyProperty NavigationContentProperty =
        DependencyProperty.Register(
            nameof(NavigationContent),
            typeof(object),
            typeof(ShellLayout),
            new PropertyMetadata(null));

    public object? NavigationContent
    {
        get => GetValue(NavigationContentProperty);
        set => SetValue(NavigationContentProperty, value);
    }

    #endregion

    #region MainContent

    public static readonly DependencyProperty MainContentProperty =
        DependencyProperty.Register(
            nameof(MainContent),
            typeof(object),
            typeof(ShellLayout),
            new PropertyMetadata(null));

    public object? MainContent
    {
        get => GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    #endregion

    #region StatusBarContent

    public static readonly DependencyProperty StatusBarContentProperty =
        DependencyProperty.Register(
            nameof(StatusBarContent),
            typeof(object),
            typeof(ShellLayout),
            new PropertyMetadata(null));

    public object? StatusBarContent
    {
        get => GetValue(StatusBarContentProperty);
        set => SetValue(StatusBarContentProperty, value);
    }

    #endregion
}