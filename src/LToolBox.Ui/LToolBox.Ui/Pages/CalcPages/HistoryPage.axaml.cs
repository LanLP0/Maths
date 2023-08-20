using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;
using LToolBox.Ui.ViewModels.CalcPageViewModels;

namespace LToolBox.Ui.Pages.CalcPages;

public partial class HistoryPage : UserControl
{
    private HistoryPageViewModel _vm;

    public HistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        _vm = (HistoryPageViewModel)DataContext!;

        base.OnInitialized();
    }

    private void ExitButtonClicked(object? sender, RoutedEventArgs e)
    {
        GoBack();
    }

    private void HistoryBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        _vm.ChosenHistory = (MathHistory)HistoryBox.SelectedItem!;

        GoBack();
    }

    private static void GoBack()
    {
        if (NavigationService.GoBack())
            return;

        NavigationService.NavigateFromContext(new CalcPageViewModel());
    }
}