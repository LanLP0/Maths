using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Styling;

namespace LToolBox.Ui.Services;

public static class ThemingService
{
    private static AppTheme _appTheme;

    public static AppTheme AppTheme
    {
        get => _appTheme;
        private set
        {
            AppState.Instance.AppTheme = value;
            _appTheme = value;
        }
    }

    public static Color AccentColor { get; private set; } = Color.Parse("#6933FFCD");

    public static event EventHandler<AppTheme>? OnThemeChange;

    /// <summary>
    ///     Switch between the Dark & Light variant of the theme
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void SwitchThemeMode()
    {
        var theme = AppTheme switch
        {
            AppTheme.Dark => AppTheme.Light,
            AppTheme.Light => AppTheme.Dark,
            _ => throw new ArgumentOutOfRangeException()
        };

        SetTheme(theme);
    }

    public static void SetTheme(AppTheme theme)
    {
        if (theme == AppTheme)
            return;

        Application.Current!.RequestedThemeVariant = theme switch
        {
            AppTheme.Dark => ThemeVariant.Dark,
            AppTheme.Light => ThemeVariant.Light,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        OnThemeChange?.Invoke(null, theme);

        AppTheme = theme;
    }

    public static void SwitchToLightMode()
    {
        if (AppTheme is AppTheme.Light)
            return;

        SwitchThemeMode();
    }

    public static void SwitchToDarkMode()
    {
        if (AppTheme is AppTheme.Dark)
            return;

        SwitchThemeMode();
    }

    public static void SetAccentColor(Color color)
    {
        AccentColor = color;
        AppState.Instance.AccentColor = color;

        Application.Current!.Resources["OverlayBackgroundColor"] = new SolidColorBrush(color, color.A / 255.0);

        var fluentStyle = (FluentAvaloniaTheme)Application.Current.Styles[0];
        fluentStyle.CustomAccentColor = color;
    }

    /// <summary>
    ///     Reload the current theme
    /// </summary>
    public static void Reload()
    {
        var theme = Application.Current!.ActualThemeVariant;
        AppTheme = theme.Key == ThemeVariant.Dark.Key ? AppTheme.Dark : AppTheme.Light;
    }
}