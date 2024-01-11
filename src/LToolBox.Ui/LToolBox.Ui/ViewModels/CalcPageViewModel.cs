using System;
using Common.Maths;
using LCalc;
using ReactiveUI;

namespace LToolBox.Ui.ViewModels;

public class CalcPageViewModel : NavViewModelBase
{
    public const string NavHeaderName = "LCalc";
    private int _caretIndex;
    private bool _displayRaw = AppState.Instance.LCalc_DisplayRaw;
    private bool _immediateOutputVisible = true;
    private string _inputText = string.Empty;
    private string _outputText = string.Empty;
    public override string NavHeader { get; } = NavHeaderName;
    public override string? IconKey { get; } = "CalculatorIcon";

    /// <summary>
    ///     Whether to display the result without the formatting or with it
    /// </summary>
    public bool DisplayRaw
    {
        get => _displayRaw;
        set
        {
            _displayRaw = value;
            AppState.Instance.LCalc_DisplayRaw = value;
            if (_displayRaw)
                ClearOutputFormatting();
        }
    }

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
            if (DisplayRaw)
                ClearOutputFormatting();
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

    public event EventHandler? InputTextChanged;

    /// <summary>
    ///     Parse the output again to remove any formatting
    /// </summary>
    private void ClearOutputFormatting()
    {
        if (string.IsNullOrWhiteSpace(_outputText))
            return;

        this.RaisePropertyChanging(nameof(OutputText));
        _outputText = Calculator.CalcRaw(_outputText).WithFormat(Format.Raw).RenderValue();
        this.RaisePropertyChanged(nameof(OutputText));
    }
}