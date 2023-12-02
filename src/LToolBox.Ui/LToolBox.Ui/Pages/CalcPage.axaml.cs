using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Common;
using LCalc;
using LToolBox.Ui.Extension;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;
using LToolBox.Ui.ViewModels.CalcPageViewModels;

namespace LToolBox.Ui.Pages;

public partial class CalcPage : UserControl
{
    // Used to indicate caret pos
    private const char ZeroWidthUnicode = '​';
    private const int MaxHistoryCount = 20;
    
    private readonly bool _isDesktop;
    private bool _isTextInputEventBeingHandled = false;
    private readonly HistoryPageViewModel _historyVm = new();
    
    private double _prevAns = double.NaN;
    private CalcResult _result;

    private CalcPageViewModel _vm = null!;

    public CalcPage()
    {
        InitializeComponent();

        Kb1.KeyClicked.Subscribe(Observer.Create<string>(Kb1_KeyClicked));
        Kb2.KeyClicked.Subscribe(Observer.Create<string>(Kb2_KeyClicked));

        // TODO: Windows Phones & Linux Phones
        // _isDesktop = OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() ||
        //     OperatingSystem.IsMacCatalyst();

        if (_isDesktop)
        {
            OskInput.IsEnabled = false;
            KeyboardButton.Hide();
            KeyboardButton.IsEnabled = false;
        }
    }

    protected override void OnInitialized()
    {
        _vm = (CalcPageViewModel)DataContext!;
        _vm.InputTextChanged += InputTextChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;
        topLevel.TextInput += Page_OnTextInput;
        topLevel.KeyDown += Page_OnKeyDown;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        MainInput.ShowCaret();
        if (OskInput.IsVisible)
            OskInput.Focus();

        LoadHistory();
    }

    private void LoadHistory()
    {
        if (_historyVm.ChosenHistory is null)
            return;

        SwitchToInputLayout();

        _vm.InputText = _historyVm.ChosenHistory.Math;
        _vm.OutputText = _historyVm.ChosenHistory.Result;

        _historyVm.ChosenHistory = null;
        ShowStepsButton.IsVisible = false;
        _vm.CaretIndex = _vm.InputText.Length;
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
    
    // TODO Kb1 & Kb2 KeyClicked
    // Simplify cases by only make them use a single string
    // which contains the text and cursor position marked by
    // the {ZeroWidthUnicode}, which the first one is the
    // initial cursor position.   (Create method)

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
                var caretIndex = _vm.CaretIndex;
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
                SwitchPanel.SetContentIndex(1);
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
                if (_isDesktop)
                {
                    AddText("[()=]");
                    _vm.CaretIndex -= 4;
                    SwitchOsk();
                    break;
                }
                
                AddText($"[({ZeroWidthUnicode})={ZeroWidthUnicode}]");
                _vm.CaretIndex -= 6;
                SwitchOsk();
                break;
            case "$fncall":
                if (_isDesktop)
                {
                    AddText("()");
                    _vm.CaretIndex -= 2;
                    SwitchOsk();
                    break;
                }
                AddText($"({ZeroWidthUnicode})");
                _vm.CaretIndex -= 3;
                SwitchOsk();
                break;
            case "$assign":
                if (_isDesktop)
                {
                    AddText("&=");
                    _vm.CaretIndex--;
                    SwitchOsk();
                    break;
                }
                
                AddText($"&={ZeroWidthUnicode}");
                _vm.CaretIndex -= 2;
                SwitchOsk();
                break;
            case "$left":
                if (_vm.CaretIndex <= 0)
                    break;

                _vm.CaretIndex--;
                break;
            case "$right":
                if (_vm.CaretIndex >= _vm.InputText.Length)
                    break;

                _vm.CaretIndex++;
                break;
            case "$home":
                _vm.CaretIndex = 0;
                break;
            case "$end":
                _vm.CaretIndex = _vm.InputText.Length;
                break;
            case "$switch":
                SwitchPanel.SetContentIndex(0);
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
            _vm.CaretIndex += s.Length;

            return;
        }

        var caretIndex = _vm.CaretIndex;
        if (prevText.Length == caretIndex)
        {
            _vm.InputText += s;
            _vm.CaretIndex += s.Length;

            return;
        }

        var first = prevText.Slice(0, caretIndex);
        var second = prevText.Slice(caretIndex);

        var buffer = new ValueStringBuilder(stackalloc char[prevText.Length + s.Length]);

        buffer.Append(first);
        buffer.Append(s);
        buffer.Append(second);

        var newText = buffer.ToString();

        _vm.InputText = newText;
        _vm.CaretIndex += s.Length;
    }

    private void KeyboardButton_OnTapped(object? sender, TappedEventArgs e)
    {
        SwitchOsk();
    }

    private void OskInput_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        var pos = _vm.InputText.IndexOf(ZeroWidthUnicode);
        if (pos is -1)
        {
            SwitchOsk();
            return;
        }

        _vm.CaretIndex = pos + 1;
        DeleteOne(false);
        var nextPos = _vm.InputText.IndexOf(ZeroWidthUnicode, pos);
        if (nextPos is -1)
        {
            SwitchOsk();
            return;
        }

        OskInput.Focus();
    }

    private void SwitchOsk()
    {
        if (_isDesktop)
            return;

        SwitchToInputLayout();

        if (MainInput.IsVisible)
        {
            KeyboardButton.Hide();
            CalculatorButton.Show();

            OskInput.Show();
            MainInput.Hide();
            MainInput.Hide();

            OskInput.Focus();
            return;
        }

        KeyboardButton.Show();
        CalculatorButton.Hide();

        MainInput.Show();
        OskInput.Hide();
        OskInput.Hide();
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

    private void DeleteOne(bool switchToInput = true)
    {
        if (switchToInput)
            SwitchToInputLayout();
        
        if (string.IsNullOrEmpty(_vm.InputText))
            return;

        var caretIndex = _vm.CaretIndex;
        if (caretIndex is 0)
            return;

        var text = _vm.InputText.AsSpan();
        if (caretIndex == text.Length)
        {
            _vm.InputText = text.Slice(0, --caretIndex).ToString();
            _vm.CaretIndex = caretIndex;
            return;
        }

        var first = text.Slice(0, caretIndex - 1);
        var second = text.Slice(caretIndex);

        var buffer = new ValueStringBuilder(stackalloc char[_vm.InputText.Length - 1]);
        buffer.Append(first);
        buffer.Append(second);

        _vm.InputText = buffer.ToString();
        _vm.CaretIndex--;
    }

    private void DeleteAll()
    {
        _vm.InputText = string.Empty;
        _vm.OutputText = string.Empty;
        
        SwitchToInputLayout();
    }

    private void Submit()
    {
        var math = _vm.InputText;
        if (string.IsNullOrWhiteSpace(math))
            return;

        const CalculatorOption option = CalculatorOption.LaTeX | CalculatorOption.NoLaTeXDoc | CalculatorOption.Step |
            CalculatorOption.VariableAllowed | CalculatorOption.Render |
            CalculatorOption.CompareAllowed | CalculatorOption.CalculatorOptionAllowed;
        var result = Calculator.CalcRaw(math, option, _prevAns);
        if (result.Faulted)
        {
            _vm.OutputText = result.Exception!.Message;

            ShowImmediateErrorOutput();
            return;
        }
        
        _vm.InputText = string.Empty;
        _vm.OutputText = math;

        _result = result;

        ResultOutput.Text = result.RenderValue();
        _prevAns = result.IsNumber ? result.Number!.Value : double.NaN;

        var history = new MathHistory(math, ResultOutput.Text);
        AddHistory(history);

        SwitchLayout(false);

        ShowStepsButton.Show();
    }

    private void ShowImmediateErrorOutput()
    {
        _vm.ImmediateOutputVisible = false;
    }

    private void AddHistory(MathHistory history)
    {
        HistoryButton.Show();

        _historyVm.History.Insert(0, history);
        if (_historyVm.History.Count > MaxHistoryCount)
        {
            var needToRemove = _historyVm.History.Count - MaxHistoryCount;

            _historyVm.History.RemoveRange(_historyVm.History.Count - needToRemove, needToRemove);
        }
    }

    private void SwitchToInputLayout()
    {
        if (!ResultLayout.IsVisible)
            return;

        SwitchLayout(true);
    }

    private void SwitchLayout(bool isInput)
    {
        ResultLayout.IsVisible = !isInput;
        InputLayout.IsVisible = isInput;
        InputLayout.IsEnabled = isInput;

        if (isInput)
        {
            if (OskInput.IsVisible)
                OskInput.Focus();

            return;
        }

        Focus();
    }

    private void ShowStepsButtonClicked(object? sender, RoutedEventArgs e)
    {
        NavigationService.NavigateFromContext(new StepsPageViewModel(_result));
    }

    private void HistoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        NavigationService.NavigateFromContext(_historyVm);
    }
    
    private void Page_OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (_isTextInputEventBeingHandled)
            return;

        _isTextInputEventBeingHandled = true;
        
        if (string.IsNullOrEmpty(e.Text))
        {
            _isTextInputEventBeingHandled = false;
            return;
        }

        // if the user is not on this page, return
        if (!ReferenceEquals(NavigationService.Frame.Content, this))
        {
            _isTextInputEventBeingHandled = false;
            return;
        }

        if (OskInput.IsVisible)
        {
            _isTextInputEventBeingHandled = false;
            return;
        }

        // Check if there's a handler for this event
        var eve = new TextInputEventArgs
        {
            Text = e.Text,
            Route = RoutingStrategies.Tunnel,
            RoutedEvent = TextInputEvent,
            Source = this
        };
        MainPanel.RaiseEvent(eve);
        if (eve.Handled)
        {
            _isTextInputEventBeingHandled = false;
            e.Handled = true;
            return;
        }

        // If not, handle it

        AddText(e.Text);
        e.Handled = true;
        _isTextInputEventBeingHandled = false;
    }
    
    private void Page_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_isTextInputEventBeingHandled)
            return;

        _isTextInputEventBeingHandled = true;
        
        // var text = $"{e.Key} {e.KeyModifiers}";
        // _vm.InputText = text;
        // return;
        
        // if the user is not on this page
        if (!ReferenceEquals(NavigationService.Frame.Content, this))
        {
            _isTextInputEventBeingHandled = false;
            return;
        }

        if (OskInput.IsVisible)
        {
            _isTextInputEventBeingHandled = false;
            return;
        }

        // Check if there's a handler for this event
        var eve = new KeyEventArgs
        {
            Key = e.Key,
            KeyModifiers = e.KeyModifiers,
            Route = RoutingStrategies.Tunnel,
            RoutedEvent = KeyDownEvent,
            Source = this
        };
        MainPanel.RaiseEvent(eve);
        if (eve.Handled)
        {
            _isTextInputEventBeingHandled = false;
            e.Handled = true;
            return;
        }

        // If not, handle it

        var shift = e.KeyModifiers == KeyModifiers.Shift;

        // Only process if KeyModifiers is None or Shift
        if (!(e.KeyModifiers is 0 || shift))
        {
            _isTextInputEventBeingHandled = false;
            return;
        }

        if (ResultLayout.IsVisible)
        {
            if (e.Key is not (Key.Space or Key.Home or Key.Escape or Key.Back))
            {
                _isTextInputEventBeingHandled = false;
                return;
            }

            e.Handled = true;

            // Go back to input mode
            SwitchLayout(true);
        }
        else
        {
            switch (e.Key)
            {
                case Key.Return:
                    e.Handled = true;

                    Submit();
                    break;
                case Key.Back: // Backspace
                    e.Handled = true;

                    DeleteOne();
                    break;
                case Key.Delete:
                    e.Handled = true;
                    if (!shift)
                    {
                        if (_vm.CaretIndex >= _vm.InputText.Length)
                            break;

                        _vm.CaretIndex++;
                        DeleteOne();
                        break;
                    }

                    DeleteAll();
                    break;
                case Key.Left:
                    if (_vm.CaretIndex <= 0)
                        break;

                    _vm.CaretIndex--;
                    break;
                case Key.Right:
                    if (_vm.CaretIndex >= _vm.InputText.Length)
                        break;

                    _vm.CaretIndex++;
                    break;
            }
        }
        
        _isTextInputEventBeingHandled = false;
    }
}