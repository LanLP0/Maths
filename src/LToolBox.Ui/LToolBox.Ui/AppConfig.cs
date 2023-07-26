using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Styling;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui;

[DataContract]
public class AppConfig
{
    [IgnoreDataMember] public static AppConfig Instance;

    [DataMember] public ThemeVariant AppTheme { get; set; } = Application.Current!.ActualThemeVariant;

    [DataMember] public string PageName { get; set; } = CalcViewModel.NavHeaderName;
}