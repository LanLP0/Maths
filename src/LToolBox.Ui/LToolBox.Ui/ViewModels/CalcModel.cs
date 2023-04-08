using System.Collections.ObjectModel;
using Avalonia.Controls;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class CalcModel : ViewModelBase
{
    private string _math = string.Empty;
    public ObservableCollection<ListBoxItem> Histories { get; } = new();

    public string Math
    {
        get => _math;
        set => this.RaiseAndSetIfChanged(ref _math, value);
    }
}