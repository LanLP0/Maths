using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using LCalc;
using LToolBox.Ui.Extension;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;
using LToolBox.Ui.ViewModels.CalcPageViewModels;

namespace LToolBox.Ui.Pages;

public sealed partial class CalcPage : UserControl
{
    private const int MaxHistoryLenght = 25;
    private double _prevAns = double.NaN;
    private CalcResult _result;
    private CalcPageViewModel? _vm;

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
        _vm = (CalcPageViewModel)DataContext!;
    }

    private void MathInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;

        if (string.IsNullOrWhiteSpace(MathInput.Text))
            return;

        MathDisplay.Text = _vm!.Math;

        const CalculatorOption option = CalculatorOption.LaTeX | CalculatorOption.NoLaTeXDoc | CalculatorOption.Step |
                                        CalculatorOption.VariableAllowed | CalculatorOption.Render |
                                        CalculatorOption.CompareAllowed | CalculatorOption.CalculatorOptionAllowed;
        var result = Calculator.CalcRaw(_vm.Math, option, _prevAns);

        if (result.Faulted) // Error
        {
            ErrorDisplay.Text = result.Exception!.Message; // Display error
            ErrorDisplay.Show();

            FocusInputBox();
            return;
        }

        if (result.Steps is not null)
        {
            _result = result;
            ShowStepsButton.Show();
        }

        if (result.IsNumber)
            _prevAns = result.Number!.Value;

        if (RawValueToggle.IsChecked!.Value)
            result.Format = Format.Raw;
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

        ErrorDisplay.Hide();
        ShowStepsButton.Hide();

        MathDisplay.Text = string.Empty; // Clear math display
        var result = Calculator.CalcRaw(_vm!.Math);

        if (result.Faulted) // Ignore error
            return;

        if (RawValueToggle.IsChecked!.Value)
            result.Format = Format.Raw;
        var resultText = result.RenderValue();

        ResultDisplay.Text = resultText;
    }

    private void ShowStepsButtonClicked(object? sender, RoutedEventArgs e)
    {
        NavigationService.NavigateFromContext(new StepsPageViewModel(_result));
    }
}