// ReSharper disable InconsistentNaming
using Avalonia.Media;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui;

public class AppState
{
    public static AppState Instance = new();

    public bool LCalc_DisplayRaw { get; set; } = false;

    public AppTheme AppTheme { get; set; } = ThemingService.AppTheme;

    public Color AccentColor { get; set; } = ThemingService.AccentColor;

    public string PageName { get; set; } = CalcPageViewModel.NavHeaderName;
}