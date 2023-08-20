using System;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public class CalcPageViewModel : NavViewModelBase
{
    public const string NavHeaderName = "LCalc";
    private int _caretIndex;
    private string _inputText = string.Empty;
    private string _outputText = string.Empty;
    public override string NavHeader { get; } = NavHeaderName;
    public override string? IconKey { get; } = "CalculatorIcon";

    public string InputText
    {
        get => _inputText;
        set
        {
            if (_inputText == value)
                return;

            _inputText = value;

            this.RaisePropertyChanged();
            InputTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string OutputText
    {
        get => _outputText;
        set => this.RaiseAndSetIfChanged(ref _outputText, value);
    }

    public int CaretIndex
    {
        get => _caretIndex;
        set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
    }

    public event EventHandler? InputTextChanged;
}