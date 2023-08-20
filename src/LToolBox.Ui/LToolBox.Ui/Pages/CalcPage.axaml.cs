using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Common;
using LCalc;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;
using LToolBox.Ui.ViewModels.CalcPageViewModels;

namespace LToolBox.Ui.Pages;

public partial class CalcPage : UserControl
{
    private const int MaxHistoryCount = 20;
    
    private CalcPageViewModel _vm = null!;
    private readonly bool _isDesktop;
    private double _prevAns = double.NaN;
    private CalcResult _result;
    private HistoryPageViewModel _historyVm = new();

    public CalcPage()
    {
        InitializeComponent();

        Kb1.KeyClicked.Subscribe(Observer.Create<string>(Kb1_KeyClicked));
        Kb2.KeyClicked.Subscribe(Observer.Create<string>(Kb2_KeyClicked));

        var isDesktop = false; // TODO: add check

        _isDesktop = isDesktop;
        if (isDesktop)
        {
            OskInput.IsVisible = true;
            OskInput.RemoveHandler(LostFocusEvent, OskInput_OnLostFocus);

            KeyboardButton.IsVisible = false;
            KeyboardButton.IsEnabled = false;
            MainInput.IsVisible = false;
            MainInput.IsEnabled = false;
        }
    }

    protected override void OnInitialized()
    {
        _vm = (CalcPageViewModel)DataContext!;
        _vm.InputTextChanged += InputTextChanged;

        base.OnInitialized();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        MainInput.ShowCaret();
        if (_historyVm.ChosenHistory is null)
            return;
        
        SwitchToInputLayout();

        _vm.InputText = _historyVm.ChosenHistory.Math;
        _vm.OutputText = _historyVm.ChosenHistory.Result;
        
        MainInput.CaretIndex = _vm.InputText.Length;
    }

    private void InputTextChanged(object? sender, EventArgs e)
    {
        SwitchToInputLayout();
        
        var math = _vm.InputText;
        if (string.IsNullOrWhiteSpace(math))
            return;

        Task.Run(() =>
        {
            var result = Calculator.CalcRaw(math, prevAns: _prevAns);
            if (result.Faulted)
                return;

            _vm.OutputText = result.RenderValue();
        }).Wait(TimeSpan.FromMilliseconds(100));
    }

    private void SwitchToInputLayout()
    {
        if (!ResultLayout.IsVisible)
            return;

        _vm.OutputText = string.Empty;
        ResultLayout.IsVisible = false;
        InputLayout.IsVisible = true;
        ShowStepsButton.IsVisible = false;
    }

    private void Kb1_KeyClicked(string key)
    {
        if (!key.StartsWith('$'))
        {
            AddText(key);
            return;
        }

        switch (key)
        {
            case "$braces":
                var caretIndex = MainInput.CaretIndex;
                if (caretIndex is 0)
                {
                    AddText("(");
                    break;
                }

                var prevChar = _vm.InputText[caretIndex - 1];
                if (prevChar is '+' or '-' or '*' or '/' or '^' or
                    '%' or '|' or '&' or '~' or '>' or '=' or '<' or '(')
                {
                    AddText("(");
                    break;
                }

                var before = _vm.InputText.AsSpan().Slice(0, caretIndex);
                
                var level = 0;
                foreach (var c in before)
                {
                    if (c is '(')
                    {
                        level++;
                        continue;
                    }

                    if (c is ')')
                    {
                        level--;
                        continue;
                    }
                }

                if (level <= 0)
                {
                    AddText("(");
                    break;
                }
                
                AddText(")");
                break;
            case "$enter":
                Submit();
                break;
            case "$switch":
                Kb1Vb.IsVisible = false;
                Kb2Vb.IsVisible = true;
                break;
        }
    }

    private void Kb2_KeyClicked(string key)
    {
        if (!key.StartsWith('$'))
        {
            AddText(key);
            return;
        }

        switch (key)
        {
            case "$cfn":
                AddText("[()=]");
                MainInput.CaretIndex -= 4;
                SwitchOsk();
                break;
            case "$fncall":
                AddText("()");
                MainInput.CaretIndex -= 2;
                SwitchOsk();
                break;
            case "$assign":
                AddText("&=");
                MainInput.CaretIndex -= 1;
                SwitchOsk();
                break;
            case "$switch":
                Kb2Vb.IsVisible = false;
                Kb1Vb.IsVisible = true;
                break;
            case "$enter":
                Submit();
                break;
        }
    }

    private void AddText(string s)
    {
        SwitchToInputLayout();

        var prevText = _vm.InputText.AsSpan();
        if (prevText.Length is 0)
        {
            _vm.InputText = s;
            MainInput.CaretIndex += s.Length;

            return;
        }

        var pos = MainInput.CaretIndex;

        var first = prevText.Slice(0, pos);
        var second = prevText.Slice(pos);

        var buffer = new ValueStringBuilder(stackalloc char[prevText.Length + s.Length]);

        buffer.Append(first);
        buffer.Append(s);
        buffer.Append(second);

        var newText = buffer.ToString();

        _vm.InputText = newText;
        MainInput.CaretIndex += s.Length;
    }

    private void KeyboardButton_OnTapped(object? sender, TappedEventArgs e)
    {
        SwitchOsk();
    }

    private void OskInput_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        SwitchOsk();
    }

    private void SwitchOsk()
    {
        if (_isDesktop)
            return;
        
        SwitchToInputLayout();
        
        if (MainInput.IsVisible)
        {
            KeyboardButton.IsVisible = false;
            CalculatorButton.IsVisible = true;

            OskInput.IsVisible = true;
            MainInput.IsVisible = false;

            OskInput.Focus();
            OskInput.CaretIndex = MainInput.CaretIndex;
            return;
        }

        KeyboardButton.IsVisible = true;
        CalculatorButton.IsVisible = false;

        MainInput.IsVisible = true;
        OskInput.IsVisible = false;

        MainInput.CaretIndex = OskInput.CaretIndex;
    }

    private void MainInput_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(MainInput);

        MainInput.MoveCaretToPoint(pos);
    }
    
    private void DeleteAllButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DeleteAll();
    }

    private void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DeleteOne();
    }

    private void DeleteOne()
    {
        SwitchToInputLayout();
        if (string.IsNullOrEmpty(_vm.InputText))
            return;
        
        // This will always be MainInput
        var caretIndex = MainInput.CaretIndex;
        if (caretIndex is 0)
            return;

        var text = _vm.InputText.AsSpan();
        var first = text.Slice(0, caretIndex - 1);
        var second = text.Slice(caretIndex);

        var buffer = new ValueStringBuilder(stackalloc char[_vm.InputText.Length - 1]);
        buffer.Append(first);
        buffer.Append(second);

        _vm.InputText = buffer.ToString();
        MainInput.CaretIndex--;
    }
    
    private void DeleteAll()
    {
        SwitchToInputLayout();
        _vm.InputText = string.Empty;
        _vm.OutputText = string.Empty;
    }

    private void Submit()
    {
        var math = _vm.InputText;
        if (string.IsNullOrWhiteSpace(math))
            return;

        _vm.OutputText = math;

        const CalculatorOption option = CalculatorOption.LaTeX | CalculatorOption.NoLaTeXDoc | CalculatorOption.Step |
            CalculatorOption.VariableAllowed | CalculatorOption.Render |
            CalculatorOption.CompareAllowed | CalculatorOption.CalculatorOptionAllowed;
        var result = Calculator.CalcRaw(math, option, _prevAns);
        if (result.Faulted)
        {
            ErrorOutput.Text = result.Exception!.Message;
            
            ResultOutput.IsVisible = false;
            ErrorOutput.IsVisible = true;
            
            ResultLayout.IsVisible = true;
            InputLayout.IsVisible = false;
            
            return;
        }
        
        _vm.InputText = string.Empty;
        _result = result;

        ResultOutput.Text = result.RenderValue();
        if (result.IsNumber)
            _prevAns = result.Number!.Value;
        
        _historyVm.History.Insert(0, new MathHistory(math, ResultOutput.Text));
        if (_historyVm.History.Count > MaxHistoryCount)
        {
            var needToRemove = _historyVm.History.Count - MaxHistoryCount;
            
            _historyVm.History.RemoveRange(_historyVm.History.Count - needToRemove, needToRemove);
        }

        ResultOutput.IsVisible = true;
        ErrorOutput.IsVisible = false;

        ResultLayout.IsVisible = true;
        InputLayout.IsVisible = false;

        ShowStepsButton.IsVisible = true;
    }

    private void ShowStepsButtonClicked(object? sender, RoutedEventArgs e)
    {
        NavigationService.NavigateFromContext(new StepsPageViewModel(_result));
    }

    private void HistoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        NavigationService.NavigateFromContext(_historyVm);
    }
}