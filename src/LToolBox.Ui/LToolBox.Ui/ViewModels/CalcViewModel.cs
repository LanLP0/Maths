using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public sealed class CalcViewModel : NavViewModelBase
{
    public const string NavHeaderName = "LCalc";
    private string _math = string.Empty;

    public string Math
    {
        get => _math;
        set => this.RaiseAndSetIfChanged(ref _math, value);
    }

    public override string NavHeader { get; } = NavHeaderName;

    public override string? IconKey { get; } = "CalculatorIcon";
}