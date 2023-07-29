using Avalonia.Media;
using LToolBox.Ui.Services;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class SettingsViewModel : NavViewModelBase
{
    private Color _accentColor = ThemingService.AccentColor;
    private AppTheme _currentAppTheme = ThemingService.AppTheme;

    public SettingsViewModel()
    {
        ThemingService.OnThemeChange += OnThemeChange;
    }

    public override string NavHeader { get; } = "Settings";
    public override string? IconKey { get; } = "SettingsIcon";
    public override bool IsFooter { get; } = true;

    public AppTheme CurrentAppTheme
    {
        get => _currentAppTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentAppTheme, value);
            ThemingService.SetTheme(_currentAppTheme);
        }
    }

    public AppTheme[] AppThemes { get; } = { AppTheme.Light, AppTheme.Dark };

    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            this.RaiseAndSetIfChanged(ref _accentColor, value);
            ThemingService.SetAccentColor(value);
        }
    }

    private void OnThemeChange(object? sender, AppTheme e)
    {
        this.RaiseAndSetIfChanged(ref _currentAppTheme, e, nameof(CurrentAppTheme));
    }
}