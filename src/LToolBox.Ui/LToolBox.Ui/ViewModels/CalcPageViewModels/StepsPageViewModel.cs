using System;
using Avalonia.Controls;
using LCalc;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels.CalcPageViewModels;

public class StepsPageViewModel : NavViewModelBase
{
    private CalcResult _result;
    
    public override string NavHeader { get; } = "Steps";
    public override string? IconKey { get; } = null;

    public string? Formula => _result.Steps;
    public string ResultText => $"Result: {_result.RenderValue()}";

    public StepsPageViewModel()
    {
        if (!Design.IsDesignMode)
            throw new InvalidOperationException();

        _result = Calculator.CalcRaw("sigma(x, |-1|, 3*(4^2), x+1)", CalculatorOption.LaTeX);
    }

    public StepsPageViewModel(CalcResult result)
    {
        _result = result;
    }
}