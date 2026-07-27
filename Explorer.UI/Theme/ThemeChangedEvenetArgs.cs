using Explorer.UI.Helpers;

namespace Explorer.UI.Theme;

public sealed class ThemeChangedEventArgs : EventArgs
{
    public ThemeChangedEventArgs(ThemeType theme)
    {
        Theme = theme;
    }

    public ThemeType Theme { get; }

    public bool IsDark => Theme == ThemeType.Dark;
}