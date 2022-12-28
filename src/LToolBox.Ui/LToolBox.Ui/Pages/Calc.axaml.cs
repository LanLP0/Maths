using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Common.Maths.Extension;
using LToolBox.Ui.Extension;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Pages;

public sealed partial class Calc : UserControl
{
    private CalcModel _calcModel;
    private const int MaxHistoryLenght = 25;

    public Calc()
    {
        InitializeComponent();
        _calcModel = new CalcModel();
        DataContext = _calcModel;
        MathDisplay.TextTrimming = TextTrimming.CharacterEllipsis;
    }

    private void MathInput_KeyUp(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
        if (e.Key is not (Key.Enter or Key.Return))
            return;

        if (string.IsNullOrWhiteSpace(MathInput.Text))
            return;
        
        HistoryBox.BeginBatchUpdate();

        var math = MathInput.Text!;
        MathDisplay.Text = math;

        var result = LCalc.Calculator.CalcUnformatted(math, out var steps);

        if (result.IsT0)
        {
            ResultDisplay.Text = result.AsT0.Message;
            HistoryBox.EndBatchUpdate();
            MathInput.Focus();
            MathInput.MoveCaretToEnd();
            return;
        }
        
        string resultText;
        if (result.IsT1)
        {
            resultText = RawValueToggle.IsChecked!.Value ? result.AsT1.ToString(CultureInfo.InvariantCulture) : result.AsT1.Humanize();
            ResultDisplay.Text = resultText;
            _calcModel.Historys.Insert(0, new TextBlock { Text = $"{resultText}: {math}", TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis});
        }
        else if (result.IsT2)
        {
            resultText = result.AsT2.ToString();
            ResultDisplay.Text = resultText;
            _calcModel.Historys.Insert(0, new TextBlock{ Text = $"{resultText}: {math}", TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis});
        }

        if (_calcModel.Historys.Count >= MaxHistoryLenght)
        {
            _calcModel.Historys.RemoveAt(_calcModel.Historys.Count - 1);
        }

        HistoryBox.EndBatchUpdate();
        MathInput.Focus();
        MathInput.Text = string.Empty;
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
}