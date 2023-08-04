using Avalonia.Media;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui;

public class AppState
{
    public static AppState Instance = new();

    public AppState()
    {
        AppTheme = ThemingService.AppTheme;
        AccentColor = ThemingService.AccentColor;
        PageName = CalcPageViewModel.NavHeaderName;
    }

    public AppTheme AppTheme { get; set; }

    public Color AccentColor { get; set; }

    public string PageName { get; set; }
}