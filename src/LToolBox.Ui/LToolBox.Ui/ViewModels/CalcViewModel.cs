using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class CalcViewModel : NavViewModelBase
{
    private string _math = string.Empty;

    public string Math
    {
        get => _math;
        set => this.RaiseAndSetIfChanged(ref _math, value);
    }

    public override string NavHeader { get; } = "LCalc";

    public override string? IconKey { get; } = "CalculatorIcon";
}