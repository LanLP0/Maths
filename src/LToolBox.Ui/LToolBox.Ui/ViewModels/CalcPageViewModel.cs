using System;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public class CalcPageViewModel : NavViewModelBase
{
    private string _inputText = string.Empty;
    private string _outputText = string.Empty;

    public const string NavHeaderName = "LCalc";
    public override string NavHeader { get; } = NavHeaderName;
    public override string? IconKey { get; } = "CalculatorIcon";

    public event EventHandler? InputTextChanged;

    public string InputText
    {
        get => _inputText;
        set
        {
            if (_inputText == value)
                return;
            
            _inputText = value;
            
            this.RaisePropertyChanged(nameof(InputText));
            InputTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string OutputText
    {
        get => _outputText;
        set => this.RaiseAndSetIfChanged(ref _outputText, value);
    }
}