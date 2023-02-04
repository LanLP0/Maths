using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Common.Maths.Extension;
using LCalc;
using LToolBox.Ui.Extension;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Pages;

public sealed partial class Calc : UserControl
{
    private const int MaxHistoryLenght = 25;
    private const double MaxFontSize = 45.0;
    private const double MinFontSize = 10.0;
    private readonly CalcModel _calcModel;

    public Calc()
    {
        InitializeComponent();
        _calcModel = new CalcModel();
        DataContext = _calcModel;
        MathDisplay.TextTrimming = TextTrimming.CharacterEllipsis;
        MathInput.AddHandler(KeyDownEvent, MathInput_KeyDown, RoutingStrategies.Tunnel);
    }

    private void MathInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Return))
            return;

        e.Handled = true;

        if (string.IsNullOrWhiteSpace(MathInput.Text))
            return;

        MathDisplay.Text = _calcModel.Math;

        var result = Calculator.CalcRaw(_calcModel.Math, out _);

        if (result.Faulted) // Error
        {
            ResultDisplay.Text = result.AsException!.Message; // Display error

            MathInput.Focus();
            MathInput.MoveCaretToEnd();
            MathDisplay.FitContent(MaxFontSize, MinFontSize);
            return;
        }

        string resultText;
        if (result.IsDouble)
        {
            resultText = RawValueToggle.IsChecked!.Value
                ? result.AsDouble!.Value.ToString(CultureInfo.InvariantCulture)
                : result.AsDouble!.Value.Humanize();
            AddToHistory(resultText);
        }
        else
        {
            resultText = result.AsDouble!.Value.ToString(CultureInfo.InvariantCulture);
            AddToHistory(resultText);
        }

        ResultDisplay.Text = resultText;

        MathInput.Focus();
        MathInput.Text = string.Empty;
        MathDisplay.FitContent(MaxFontSize, MinFontSize);
    }

    private void AddToHistory(string resultText)
    {
        _calcModel.Historys.Insert(0,
            new TextBlock
            {
                Text = $"{resultText}: {_calcModel.Math}", TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

        if (_calcModel.Historys.Count >= MaxHistoryLenght)
            _calcModel.Historys.RemoveAt(_calcModel.Historys.Count - 1);
    }

    private void HistoryBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var text = (HistoryBox.SelectedItem as TextBlock)!.Text!;
        MathInput.Text = text.Substring(text.IndexOf(' ') + 1);
        MathInput.Focus();
        MathInput.MoveCaretToEnd();
        e.Handled = true;
    }

    private void ClearHistory(object? sender, RoutedEventArgs e)
    {
        _calcModel.Historys.Clear();
    }

    private void MathInput_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        // ResultDisplay.Text = MathInput.Text;
        // ResultDisplay.FitContent(MaxFontSize, MinFontSize);
        // return;

        if (string.IsNullOrWhiteSpace(MathInput.Text))
            return;

        MathDisplay.Text = string.Empty; // Clear math display
        var result = Calculator.CalcRaw(_calcModel.Math, out _);

        if (result.Faulted) // Ignore error
            return;

        string resultText;
        if (result.IsDouble)
        {
            resultText = RawValueToggle.IsChecked!.Value
                ? result.AsDouble!.Value.ToString(CultureInfo.InvariantCulture)
                : result.AsDouble!.Value.Humanize();
            ResultDisplay.Text = resultText;
        }
        else
        {
            resultText = result.AsBool!.Value.ToString();
            ResultDisplay.Text = resultText;
        }

        ResultDisplay.FitContent(10);
    }
}