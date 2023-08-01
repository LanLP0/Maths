using System.Runtime.Serialization;
using Avalonia.Media;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui;

[DataContract]
public class AppState
{
    [IgnoreDataMember] public static AppState Instance = new();

    [DataMember] public AppTheme AppTheme { get; set; } = ThemingService.AppTheme;

    [DataMember] public Color AccentColor { get; set; } = ThemingService.AccentColor;

    [DataMember] public string PageName { get; set; } = CalcPageViewModel.NavHeaderName;
}