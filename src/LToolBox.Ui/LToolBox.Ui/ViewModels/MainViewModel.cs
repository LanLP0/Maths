using Avalonia.Controls;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private ListBoxItem _selectedItem;

    public ListBoxItem SelectedItem
    {
        set
        {
            _selectedItem = value;
            this.RaisePropertyChanged(nameof(SelectedItemText));
        }
    }

    public string SelectedItemText => _selectedItem?.Content is TextBlock textBlock ? textBlock.Text! : "LCalc";
}