using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LToolBox.Ui.Extension;
using LToolBox.Ui.ViewModels;
using MinMaxFraction.Core;
using ReactiveUI;

namespace LToolBox.Ui.Pages;

public sealed partial class MinMaxFracPage : UserControl
{
    private const int HistoryLimit = 10;
    private MinMaxFracPageViewModel _vm;

    public MinMaxFracPage()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        _vm = (MinMaxFracPageViewModel)DataContext!;
    }

    private void InputT0_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        InputT1.Focus();
    }

    private void InputT1_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        InputT2.Focus();
    }

    private void InputT2_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        InputB0.Focus();
    }

    private void InputB0_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        InputB1.Focus();
    }

    private void InputB1_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        e.Handled = true;
        InputB2.Focus();
    }

    private void InputB2_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        // Reroute
        SubmitPressed(sender, e);
    }

    private void SubmitPressed(object? sender, RoutedEventArgs e)
    {
        ErrorDisplay.Hide();

        var fraction = new MMFraction();
        try
        {
            if (!string.IsNullOrEmpty(InputT0.Text))
                fraction.T0 = double.Parse(InputT0.Text);
            if (!string.IsNullOrEmpty(InputT1.Text))
                fraction.T1 = double.Parse(InputT1.Text);
            if (!string.IsNullOrEmpty(InputT2.Text))
                fraction.T2 = double.Parse(InputT2.Text);

            if (!string.IsNullOrEmpty(InputB0.Text))
                fraction.B0 = double.Parse(InputB0.Text);
            if (!string.IsNullOrEmpty(InputB1.Text))
                fraction.B1 = double.Parse(InputB1.Text);
            if (!string.IsNullOrEmpty(InputB2.Text))
                fraction.B2 = double.Parse(InputB2.Text);
        }
        catch (Exception exception)
        {
            var pos = exception.Message.IndexOf('\'');
            var pos2 = exception.Message.IndexOf('\'', ++pos);
            var input = exception.Message.Substring(pos, pos2 - pos);

            ErrorDisplay.Text = $"'{input}' is not a number";
            ErrorDisplay.Show();
            return;
        }

        if (!fraction.Validate())
        {
            ErrorDisplay.Text = "The fraction cannot contains a zero nominator or denominator";
            ErrorDisplay.Show();
            return;
        }

        var polynomial = fraction.Calc();
        var deltaResult = polynomial.Calc();

        var history = new MinMaxFracHistory(fraction.T0, fraction.T1, fraction.T2,
            fraction.B0, fraction.B1, fraction.B2,
            polynomial.V0.NumPart, polynomial.V1.NumPart, polynomial.V2.NumPart,
            $"Result: {deltaResult.RenderResult()}");
        
        AddToHistory(history);
        Clear();
    }

    private void AddToHistory(MinMaxFracHistory history)
    {
        HistoryBox.Items.Insert(0, history);
        if (HistoryBox.Items.Count <= HistoryLimit)
            return;

        // Should always be the last item
        HistoryBox.Items.RemoveAt(HistoryLimit);
    }

    private void HistoryBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var history = (MinMaxFracHistory)HistoryBox.SelectedItem!;

        LoadHistory(history);
    }

    private void LoadHistory(MinMaxFracHistory history)
    {
        InputT0.Text = history.T0.ToString(CultureInfo.InvariantCulture);
        InputT1.Text = history.T1.ToString(CultureInfo.InvariantCulture);
        InputT2.Text = history.T2.ToString(CultureInfo.InvariantCulture);
        
        InputB0.Text = history.B0.ToString(CultureInfo.InvariantCulture);
        InputB1.Text = history.B1.ToString(CultureInfo.InvariantCulture);
        InputB2.Text = history.B2.ToString(CultureInfo.InvariantCulture);
    }

    private void ClearButtonPressed(object? sender, RoutedEventArgs e)
    {
        Clear();
    }

    private void Clear()
    {
        ErrorDisplay.Hide();
        InputT0.Text = string.Empty;
        InputT1.Text = string.Empty;
        InputT2.Text = string.Empty;
        InputB0.Text = string.Empty;
        InputB1.Text = string.Empty;
        InputB2.Text = string.Empty;
    }

    private void ClearHistoryButtonPressed(object? sender, RoutedEventArgs e)
    {
        ClearHistory();
    }

    private void ClearHistory()
    {
        HistoryBox.Items.Clear();
    }
}