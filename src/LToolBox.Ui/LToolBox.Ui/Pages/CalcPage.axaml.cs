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

public sealed partial class CalcPage : UserControl
{
    private const int MaxHistoryLenght = 25;
    private CalcViewModel? _vm;

    public CalcPage()
    {
        InitializeComponent();
        MathDisplay.TextTrimming = TextTrimming.CharacterEllipsis;
        MathInput.AddHandler(KeyDownEvent, MathInput_KeyDown, RoutingStrategies.Tunnel);
    }

    private ItemCollection Histories => HistoryBox.Items;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _vm = (CalcViewModel)DataContext!;
    }

    private void MathInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;

        if (string.IsNullOrWhiteSpace(MathInput.Text))
            return;

        MathDisplay.Text = _vm!.Math;

        var result = Calculator.CalcRaw(_vm.Math);

        if (result.Faulted) // Error
        {
            ResultDisplay.Text = result.Exception!.Message; // Display error

            FocusInputBox();
            return;
        }

        result.Format = RawValueToggle.IsChecked!.Value ? Format.Raw : Format.Human;
        var resultText = result.RenderValue();

        ResultDisplay.Text = resultText;
        AddToHistory(resultText);

        FocusInputBox();
        MathInput.Text = string.Empty;
    }

    private void AddToHistory(string resultText)
    {
        var mathBlock = new TextBlock
        {
            Text = _vm!.Math,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextAlignment = TextAlignment.Left
        };

        var resultBlock = new TextBlock
        {
            Text = resultText,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextAlignment = TextAlignment.Right,
            Foreground = Brushes.Aqua
        };
        resultBlock.SetValue(Grid.ColumnProperty, 2);

        var item = new ListBoxItem
        {
            Content = new Grid
            {
                ColumnDefinitions = ColumnDefinitions.Parse("*, 5, Auto"),
                Children =
                {
                    mathBlock,
                    resultBlock
                }
            }
        };

        Histories.Insert(0, item);

        if (Histories.Count >= MaxHistoryLenght)
            Histories.RemoveAt(Histories.Count - 1);

        HistoryBox.InvalidateVisual();
    }

    private void HistoryBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var historyItem = (ListBoxItem)HistoryBox.SelectedItem!;
        var grid = (Grid)historyItem.Content!;
        var mathBlock = (TextBlock)grid.Children[0];
        var text = mathBlock.Text!;
        MathInput.Text = text.Substring(text.IndexOf(' ') + 1);
        FocusInputBox();
        e.Handled = true;
    }

    private void FocusInputBox()
    {
        MathInput.Focus();
        MathInput.MoveCaretToEnd();
    }

    private void ClearHistory(object? sender, RoutedEventArgs e)
    {
        Histories.Clear();
    }

    private void MathInput_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MathInput.Text))
            return;

        MathDisplay.Text = string.Empty; // Clear math display
        var result = Calculator.CalcRaw(_vm!.Math);

        if (result.Faulted) // Ignore error
            return;

        result.Format = RawValueToggle.IsChecked!.Value ? Format.Raw : Format.Human;
        var resultText = result.RenderValue();

        ResultDisplay.Text = resultText;
    }
}