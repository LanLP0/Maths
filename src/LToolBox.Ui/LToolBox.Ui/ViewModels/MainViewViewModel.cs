using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class MainViewViewModel : ViewModelBase
{
    private NavigationViewItem _selectedItem;

    public NavigationViewItem SelectedItem
    {
        get => _selectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }
    }
}