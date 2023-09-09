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
    
    public new static readonly RoutedEvent<KeyEventArgs> KeyDownEvent =
        RoutedEvent.Register<InputElement, KeyEventArgs>(
            nameof(KeyDown),
            RoutingStrategies.Tunnel);

    public new event EventHandler<KeyEventArgs>? KeyDown
    {
        add { AddHandler(KeyDownEvent, value); }
        remove { RemoveHandler(KeyDownEvent, value); }
    }

    private const int MaxHistoryCount = 20;
    private readonly bool _isDesktop;
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
        _isDesktop = false;

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
        TopLevel.GetTopLevel(this)!.KeyDown += Page_OnKeyDown;
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

        _vm.InputText = string.Empty;
        _vm.OutputText = math;

        const CalculatorOption option = CalculatorOption.LaTeX | CalculatorOption.NoLaTeXDoc | CalculatorOption.Step |
            CalculatorOption.VariableAllowed | CalculatorOption.Render |
            CalculatorOption.CompareAllowed | CalculatorOption.CalculatorOptionAllowed;
        var result = Calculator.CalcRaw(math, option, _prevAns);
        if (result.Faulted)
        {
            ErrorOutput.Text = result.Exception!.Message;

            ResultOutput.Hide();
            ErrorOutput.Show();

            SwitchLayout(false);
            ShowStepsButton.Hide();

            return;
        }

        _result = result;

        ResultOutput.Text = result.RenderValue();
        _prevAns = result.IsNumber ? result.Number!.Value : double.NaN;

        var history = new MathHistory(math, ResultOutput.Text);
        AddHistory(history);

        ResultOutput.Show();
        ErrorOutput.Hide();

        SwitchLayout(false);

        ShowStepsButton.Show();
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

    private void Page_OnKeyDown(object? sender, KeyEventArgs e)
    {
        // var text = $"{e.Key} {e.KeyModifiers}";
        // _vm.InputText = text;
        // return;

        if (!ReferenceEquals(NavigationService.Frame.Content, this))
            return;

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
            return;

        var shift = e.KeyModifiers == KeyModifiers.Shift;

        // Only process if KeyModifiers is None or Shift
        if (!(e.KeyModifiers is 0 || shift))
            return;

        if (ResultLayout.IsVisible)
            switch (e.Key)
            {
                case Key.Space:
                case Key.Home:
                case Key.Escape:
                case Key.Back:
                    e.Handled = true;

                    SwitchLayout(true);
                    break;
                case Key.A:
                case Key.B:
                case Key.C:
                case Key.D:
                case Key.E:
                case Key.F:
                case Key.G:
                case Key.H:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.L:
                case Key.M:
                case Key.N:
                case Key.O:
                case Key.P:
                case Key.Q:
                case Key.R:
                case Key.S:
                case Key.T:
                case Key.U:
                case Key.V:
                case Key.W:
                case Key.X:
                case Key.Y:
                case Key.Z:
                    e.Handled = true;

                    var key = shift
                        ? e.Key.ToString()
                        : e.Key.ToString().ToLowerInvariant();

                    AddText(key);
                    break;
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    e.Handled = true;

                    var number1 = e.Key.ToString()[^1];
                    AddText(number1.ToString());

                    break;
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D8:
                    if (shift)
                        break;

                    e.Handled = true;

                    var number2 = e.Key.ToString()[^1];
                    AddText(number2.ToString());

                    break;
                case Key.D7:
                    e.Handled = true;

                    AddText(shift ? "&" : "7");
                    break;
                case Key.D9:
                    e.Handled = true;

                    AddText(shift ? "(" : "9");
                    break;
                case Key.Subtract:
                    e.Handled = true;

                    AddText("-");
                    break;
                case Key.OemMinus:
                    if (shift)
                        break;

                    e.Handled = true;

                    AddText("-");
                    break;
                case Key.OemTilde:
                    if (!shift)
                        break;

                    e.Handled = true;

                    AddText("~");
                    break;
                case Key.OemPipe:
                    if (!shift)
                        break;

                    e.Handled = true;

                    AddText("|");
                    break;
                case Key.OemPeriod:
                    if (shift)
                        break;

                    e.Handled = true;

                    AddText(".");
                    break;
            }
        else
            switch (e.Key)
            {
                case Key.Return:
                    e.Handled = true;

                    Submit();
                    break;
                case Key.A:
                case Key.B:
                case Key.C:
                case Key.D:
                case Key.E:
                case Key.F:
                case Key.G:
                case Key.H:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.L:
                case Key.M:
                case Key.N:
                case Key.O:
                case Key.P:
                case Key.Q:
                case Key.R:
                case Key.S:
                case Key.T:
                case Key.U:
                case Key.V:
                case Key.W:
                case Key.X:
                case Key.Y:
                case Key.Z:
                    e.Handled = true;

                    var key = shift
                        ? e.Key.ToString()
                        : e.Key.ToString().ToLowerInvariant();

                    AddText(key);
                    break;
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    e.Handled = true;

                    var number = e.Key.ToString()[^1];
                    AddText(number.ToString());

                    break;
                case Key.D2:
                case Key.D3:
                case Key.D4:
                    if (shift)
                        break;

                    e.Handled = true;

                    var number2 = e.Key.ToString()[^1];
                    AddText(number2.ToString());

                    break;
                case Key.D1:
                    e.Handled = true;

                    AddText(shift ? "!" : "1");
                    break;
                case Key.D5:
                    e.Handled = true;

                    AddText(shift ? "%" : "5");
                    break;
                case Key.D6:
                    e.Handled = true;

                    AddText(shift ? "^" : "6");
                    break;
                case Key.D7:
                    e.Handled = true;

                    AddText(shift ? "&" : "7");
                    break;
                case Key.D8:
                    e.Handled = true;

                    AddText(shift ? "*" : "8");
                    break;
                case Key.D9:
                    e.Handled = true;

                    AddText(shift ? "(" : "9");
                    break;
                case Key.D0:
                    e.Handled = true;

                    AddText(shift ? ")" : "0");
                    break;
                case Key.OemComma:
                    e.Handled = true;

                    AddText(shift ? "<" : ",");
                    break;
                case Key.OemPeriod:
                    e.Handled = true;

                    AddText(shift ? ">" : ".");
                    break;
                case Key.Subtract:
                    e.Handled = true;

                    AddText("-");
                    break;
                case Key.OemMinus:
                    if (shift)
                        break;

                    e.Handled = true;

                    AddText("-");
                    break;
                case Key.OemTilde:
                    if (!shift)
                        break;

                    e.Handled = true;

                    AddText("~");
                    break;
                case Key.OemPipe:
                    if (!shift)
                        break;

                    e.Handled = true;

                    AddText("|");
                    break;
                case Key.Oem2: // The `?/` key
                    if (!shift)
                        break;

                    e.Handled = true;

                    AddText("/");
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
}