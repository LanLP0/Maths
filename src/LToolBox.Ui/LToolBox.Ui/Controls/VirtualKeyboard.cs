using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;

namespace LToolBox.Ui.Controls;

public class VirtualKeyboard : Grid
{
    public new static readonly AttachedProperty<int> ColumnProperty = Grid.ColumnProperty;
    
    public new static int GetColumn(Control element)
    {
        return element.GetValue(ColumnProperty);
    }

    public new static void SetColumn(Control element, int value)
    {
        element.SetValue(ColumnProperty, value);
    }
    
    public new static readonly AttachedProperty<int> RowProperty = Grid.RowProperty;
    
    public new static int GetRow(Control element)
    {
        return element.GetValue(RowProperty);
    }

    public new static void SetRow(Control element, int value)
    {
        element.SetValue(RowProperty, value);
    }
    
    public Subject<string> KeyClicked = new();
    
    protected override void OnInitialized()
    {
        DataContext = this;
        
        base.OnInitialized();
    }
}