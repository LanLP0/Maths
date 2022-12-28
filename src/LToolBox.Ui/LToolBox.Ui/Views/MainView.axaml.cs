using Avalonia.Controls;
using Avalonia.Input;

namespace LToolBox.Ui.Views;

public sealed partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    
    private void AppChooser_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;

        try
        {
            
        }
        catch
        {
            // ignored
        }
    }
}