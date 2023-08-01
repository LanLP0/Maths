using Avalonia.Controls;
using Avalonia.Interactivity;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Pages.CalcPages;

public partial class StepsPage : UserControl
{
    public StepsPage()
    {
        InitializeComponent();
        ThemingService.OnThemeChanged += OnThemeChanged;
    }

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