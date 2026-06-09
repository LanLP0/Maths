using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Common.Maths.Extension;
using LToolBox.Ui.Utils.Extension;
using LToolBox.Ui.ViewModels;
using MathNet.Numerics;

namespace LToolBox.Ui.Pages;

public sealed partial class PolynomialPage : UserControl
{
    private const int HistoryLimit = 10;
    private PolynomialPageViewModel _vm;
    private readonly TextBox[] _inputs;

    public PolynomialPage()
    {
        InitializeComponent();
        _inputs =
        [
            Input0,
            Input1,
            Input2,
            Input3,
            Input4,
            Input5
        ];
    }

    protected override void OnInitialized()
    {
        _vm = (PolynomialPageViewModel)DataContext!;
        DegreeSelector.SelectedIndex = 0;
    }

    private int GetDegree()
    {
        return DegreeSelector.SelectedIndex + 2;
    }

    private void SetDegree(int degree)
    {
        DegreeSelector.SelectedIndex = degree - 2;
    }

    private void OnDegreeChanged(object? sender, SelectionChangedEventArgs e)
    {
        var degree = GetDegree();

        if (degree >= 5)
        {
            InputGroup5.Show();
            InputGroup4.Show();
            InputGroup3.Show();

            return;
        }

        InputGroup5.Hide();

        if (degree >= 4)
        {
            InputGroup4.Show();
            InputGroup3.Show();

            return;
        }

        InputGroup4.Hide();

        if (degree >= 3)
        {
            InputGroup3.Show();
            return;
        }

        InputGroup3.Hide();
    }

    /// <summary>
    /// Get the coefficients provided by the input boxes
    /// </summary>
    /// <exception cref="System.FormatException">There is a misformatted number</exception>
    /// <returns>The array contains all the coefficients</returns>
    private double[] GetCoefficients()
    {
        var coefficients = new double[6];
        for (var i = 0; i < _inputs.Length; i++)
        {
            var input = _inputs[i];
            if (string.IsNullOrWhiteSpace(input.Text))
            {
                coefficients[i] = 0;
                continue;
            }

            coefficients[i] = double.Parse(input.Text);
        }

        return coefficients;
    }

    private void Submit()
    {
        try
        {
            var coefficients = GetCoefficients();
            if (coefficients.All(value => value is 0))
            {
                Input5.Focus();
                return;
            }

            var polynomial = new Polynomial(coefficients);
            var roots = polynomial.Roots();

            var resultText = string.Join(", ", roots.Select(root => root.Humanize()));
            AddToHistory(coefficients, $"{polynomial.ToStringDescending()} = 0\nResults: {resultText}");

            Clear();
        }
        catch
        {
            // ignored
        }
    }

    private void AddToHistory(double[] coefficients, string renderText)
    {
        var history = new PolynomialHistory
        {
            Coefficients = coefficients,
            RenderText = renderText
        };

        _vm.Histories.Insert(0, history);

        if (_vm.Histories.Count > HistoryLimit)
            _vm.Histories.RemoveAt(HistoryLimit);
        
        _vm.RaiseHistoryChanged();
    }

    private void Clear()
    {
        foreach (var input in _inputs)
        {
            input.Clear();
        }
    }

    private void SubmitPressed(object? sender, RoutedEventArgs e)
    {
        Submit();
    }

    private void ClearHistoryButtonPressed(object? sender, RoutedEventArgs e)
    {
        _vm.Histories.Clear();
        _vm.RaiseHistoryChanged();
    }

    private void ClearButtonPressed(object? sender, RoutedEventArgs e)
    {
        Clear();
    }

    private void HistoryBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var history = (PolynomialHistory)HistoryBox.SelectedItem!;

        Clear();
        LoadHistory(history);
    }

    private void LoadHistory(PolynomialHistory history)
    {
        var coefficients = history.Coefficients;
        var hasDegreeBeenSet = false;

        if (coefficients[5] is not 0)
        {
            SetDegree(5);
            hasDegreeBeenSet = true;

            Input5.Text = coefficients[5].ToString();
        }
        
        if (coefficients[4] is not 0)
        {
            if (!hasDegreeBeenSet)
            {
                SetDegree(4);
                hasDegreeBeenSet = true;
            }

            Input4.Text = coefficients[4].ToString();
        }
        
        if (coefficients[3] is not 0)
        {
            if (!hasDegreeBeenSet)
            {
                SetDegree(3);
                hasDegreeBeenSet = true;
            }

            Input3.Text = coefficients[3].ToString();
        }
        
        if (coefficients[2] is not 0)
        {
            if (!hasDegreeBeenSet)
                SetDegree(2);
            
            Input2.Text = coefficients[2].ToString();
        }

        Input1.Text = coefficients[1].ToString();
        Input0.Text = coefficients[0].ToString();
    }

    private void Input5_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        Input4.Focus();
    }

    private void Input4_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        Input3.Focus();
    }

    private void Input3_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        Input2.Focus();
    }

    private void Input2_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        Input1.Focus();
    }

    private void Input1_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        Input0.Focus();
    }

    private void Input0_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        Submit();
    }
}