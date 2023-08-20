using System.Collections.Generic;

namespace LToolBox.Ui.ViewModels.CalcPageViewModels;

public class HistoryPageViewModel : NavViewModelBase
{
    public override string NavHeader { get; } = "History";
    public override string? IconKey { get; }

    public List<MathHistory> History { get; } = new();
    public MathHistory? ChosenHistory { get; set; }
}