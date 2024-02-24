using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public class PolynomialPageViewModel : NavViewModelBase
{
    public override string NavHeader { get; } = "Polynomial";
    public override string? IconKey { get; } = "PolynomialIcon";
    
    public ObservableCollection<PolynomialHistory> Histories { get; set; } = new();

    public void RaiseHistoryChanged()
    {
        this.RaisePropertyChanged(nameof(Histories));
    }
}