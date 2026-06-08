using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class MainViewViewModel : ViewModelBase
{
    private string _headerText;
    private FANavigationViewItem _selectedItem;

    public FANavigationViewItem SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public string HeaderText
    {
        get => _headerText;
        set => this.RaiseAndSetIfChanged(ref _headerText, value);
    }
}