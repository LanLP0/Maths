using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace LToolBox.Ui.ViewModels;

public sealed class CalcModel : ViewModelBase
{
    public ObservableCollection<TextBlock> Historys { get; } = new();
}