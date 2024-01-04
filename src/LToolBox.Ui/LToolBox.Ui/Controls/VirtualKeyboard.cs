using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
        
        // Disable outer border
        foreach (var child in Children)
        {
            var c = (TemplatedControl)child;
            if (GetColumn(child) is 0)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 1);
            
            if (GetColumn(child) == ColumnDefinitions.Count - 1)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 3);
            
            if (GetRow(child) is 0)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 2);
            
            if (GetRow(child) == RowDefinitions.Count - 1)
                c.BorderThickness = c.BorderThickness.ChangeSingle(0, 4);
        }

        base.OnInitialized();
    }
}