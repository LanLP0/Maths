using System.Collections.Generic;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class MinMaxFracPageViewModel : NavViewModelBase
{
    public override string NavHeader { get; } = "MinMaxFrac";
    public override string? IconKey { get; } = "DivideIcon";
}