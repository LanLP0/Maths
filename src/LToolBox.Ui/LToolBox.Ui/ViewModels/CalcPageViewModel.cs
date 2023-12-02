using System;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public class CalcPageViewModel : NavViewModelBase
{
    private int _caretIndex;
    private string _inputText = string.Empty;
    private string _outputText = string.Empty;
    private bool _immediateOutputVisible = true;
    
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

            this.RaisePropertyChanged();
            InputTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string OutputText
    {
        get => _outputText;
        set
        {
            if (_outputText == value)
                return;

            this.RaisePropertyChanging();
            _outputText = value;
            this.RaisePropertyChanged();
            ImmediateOutputVisible = true;
        }
    }

    public bool ImmediateOutputVisible
    {
        get => _immediateOutputVisible;
        set
        {
            if (_immediateOutputVisible == value)
                return;
            
            this.RaisePropertyChanging();
            this.RaisePropertyChanging(nameof(ImmediateErrorOutputVisible));
            _immediateOutputVisible = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(ImmediateErrorOutputVisible));
        }
    }

    public bool ImmediateErrorOutputVisible => !_immediateOutputVisible;

    public int CaretIndex
    {
        get => _caretIndex;
        set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
    }
}