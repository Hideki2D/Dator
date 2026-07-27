using Explorer.UI.Helpers;
using Microsoft.Win32;
using System;
using System.Windows;

namespace Explorer.UI.Theme;

public static class ThemeManager
{
    private static readonly Uri DarkTheme =
        new("/Explorer.UI;component/Resources/Themes/Dark.xaml",
            UriKind.Relative);

    private static readonly Uri LightTheme =
        new("/Explorer.UI;component/Resources/Themes/Light.xaml",
            UriKind.Relative);

    private static ResourceDictionary? _themeDictionary;

    static ThemeManager()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    public static ThemeType RequestedTheme { get; private set; }
        = ThemeType.System;

    public static ThemeType ActualTheme { get; private set; }

    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public static void Initialize()
    {
        ApplyTheme(RequestedTheme);
    }

    public static void SetTheme(ThemeType theme)
    {
        RequestedTheme = theme;
        ApplyTheme(theme);
    }

    private static void ApplyTheme(ThemeType requested)
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() =>
                ApplyTheme(requested));

            return;
        }

        var actual =
            requested == ThemeType.System
                ? GetSystemTheme()
                : requested;

        ActualTheme = actual;

        var uri =
            actual == ThemeType.Dark
                ? DarkTheme
                : LightTheme;

        if (_themeDictionary != null)
            Application.Current.Resources.MergedDictionaries.Remove(_themeDictionary);

        _themeDictionary = new ResourceDictionary
        {
            Source = uri
        };

        Application.Current.Resources.MergedDictionaries.Insert(0, _themeDictionary);

        ThemeChanged?.Invoke(
            null,
            new ThemeChangedEventArgs(actual));
    }

    private static ThemeType GetSystemTheme()
    {
        using var key =
            Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

        var value = key?.GetValue("AppsUseLightTheme");

        if (value is int i && i == 0)
            return ThemeType.Dark;

        return ThemeType.Light;
    }

    private static void OnUserPreferenceChanged(
        object? sender,
        UserPreferenceChangedEventArgs e)
    {
        if (RequestedTheme != ThemeType.System)
            return;

        ApplyTheme(ThemeType.System);
    }
}