using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LToolBox.Ui.Extension;
using Material.Styles.Controls;
using MinMaxFraction.Core;

namespace LToolBox.Ui.Pages;

public partial class MinMaxFrac : UserControl
{
    private const int HistoryLimit = 10;
    
    public MinMaxFrac()
    {
        InitializeComponent();
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
        ExceptionDisplay.Hide();
        
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
            
            ExceptionDisplay.Text = $"'{input}' is not a number";
            ExceptionDisplay.Show();
            return;
        }

        if (!fraction.Validate())
        {
            ExceptionDisplay.Text = "The fraction cannot contains a zero nominator or denominator";
            ExceptionDisplay.Show();
            return;
        }

        var polynomial = fraction.Calc();
        var deltaResult = polynomial.Calc();

        var result = RenderResult(deltaResult, fraction);
        
        AddToHistory(result);
    }

    private void AddToHistory(ListBoxItem item)
    {
        HistoryBox.Items.Insert(0, item);
        if (HistoryBox.Items.Count <= HistoryLimit)
            return;
        
        // Should always be the last item
        HistoryBox.Items.RemoveAt(HistoryLimit);
    }

    private ListBoxItem RenderResult(MMDeltaResult deltaResult, MMFraction fraction)
    {
        return new ListBoxItem
        {
            Content = new ColorZone
            {
                FontSize = 20,
                Foreground = Brushes.White,
                Background = Brush.Parse("#30ffffff"),
                BorderBrush = Brushes.White,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock // Store history data (fraction)
                        {
                            IsVisible = false,
                            Text = $"{fraction.T0}\\{fraction.T1}\\{fraction.T2}\\{fraction.B0}\\{fraction.B1}\\{fraction.B2}"
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Text = "A  =  "
                                },
                                new StackPanel
                                {
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Margin = new Thickness(5, 0),
                                            Text = $"{fraction.T0}x^2 + {fraction.T1}x + {fraction.T2}"
                                        },
                                        new Separator
                                        {
                                            Foreground = Brushes.White,
                                            Margin = new Thickness(0)
                                        },
                                        new TextBlock
                                        {
                                            Margin = new Thickness(5, 0),
                                            Text = $"{fraction.B0}x^2 + {fraction.B1}x + {fraction.B2}"
                                        }
                                    }
                                }
                            }
                        },
                        new TextBlock
                        {
                            Text = $"{deltaResult.V0}A^2 + {deltaResult.V1}A + {deltaResult.V2} >= 0"
                        },
                        new TextBlock
                        {
                            Text = $"Result: {deltaResult.RenderResult()}"
                        }
                    }
                }
            }
        };
    }

    private void HistoryBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var historyItem = (ListBoxItem)HistoryBox.SelectedItem!;
        
        LoadHistory(historyItem);
    }

    private void LoadHistory(ListBoxItem historyItem)
    {
        var colorZone = (ColorZone)historyItem.Content!;
        var stackPanel = (StackPanel)colorZone.Content!;
        var dataTextBlock = (TextBlock)stackPanel.Children[0];
        
        var data = dataTextBlock.Text!;
        var datas = data.Split('\\');

        InputT0.Text = datas[0];
        InputT1.Text = datas[1];
        InputT2.Text = datas[2];
        InputB0.Text = datas[3];
        InputB1.Text = datas[4];
        InputB2.Text = datas[5];
    }

    private void ClearButtonPressed(object? sender, RoutedEventArgs e)
    {
        ExceptionDisplay.Hide();
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