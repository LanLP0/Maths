using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using LToolBox.Ui.Extension;

namespace LToolBox.Ui.Controls;

public class VirtualKeyboard : Grid
{
    public new static readonly AttachedProperty<int> ColumnProperty = Grid.ColumnProperty;

    public new static readonly AttachedProperty<int> RowProperty = Grid.RowProperty;

    public Subject<string> KeyClicked = new();

    public new static int GetColumn(Control element)
    {
        return element.GetValue(ColumnProperty);
    }

    public new static void SetColumn(Control element, int value)
    {
        element.SetValue(ColumnProperty, value);
    }

    public new static int GetRow(Control element)
    {
        return element.GetValue(RowProperty);
    }

    public new static void SetRow(Control element, int value)
    {
        element.SetValue(RowProperty, value);
    }

    protected override void OnInitialized()
    {
        DataContext = this;

        base.OnInitialized();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        DisableOuterBorder();
    }

    private void DisableOuterBorder()
    {
        foreach (var child in Children)
        {
            var c = (TemplatedControl)child;

            var column = GetColumn(child);
            if (column is 0)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 1);

            if (column == ColumnDefinitions.Count - 1)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 3);

            var row = GetRow(child);
            if (row is 0)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 2);

            if (row == RowDefinitions.Count - 1)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 4);
        }
    }
}