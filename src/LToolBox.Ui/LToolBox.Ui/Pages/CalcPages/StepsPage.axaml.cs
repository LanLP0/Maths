using Avalonia.Controls;
using Avalonia.Interactivity;
// using CSharpMath.Avalonia;
using LToolBox.Ui.Services;
using LToolBox.Ui.Utils.Converter;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Pages.CalcPages;

public partial class StepsPage : UserControl
{
    public StepsPage()
    {
        InitializeComponent();
        ThemingService.OnThemeChanged += OnThemeChanged;
    }

    // protected override void OnInitialized()
    // {
    //     var foregroundColor = Resources.GetResourceObservable("TextControlForeground", ActualThemeVariant,
    //         SolidColorBrushToColorConverter.ConvertStatic);
    //     
    //     FormulaBlock.Bind(MathView.TextColorProperty, foregroundColor);
    // }

    private void OnThemeChanged(object? sender, AppTheme e)
    {
        FormulaBlock.InvalidateVisual();
    }

    private void ExitButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (NavigationService.GoBack())
            return;

        NavigationService.NavigateFromContext(new CalcPageViewModel());
    }
}