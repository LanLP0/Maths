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
        try
        {
        }
        catch
        {
            // ignored
        }
    
        // LeftDrawer.IsPaneOpen = false;
        LeftDrawer.OptionalCloseLeftDrawer();
    }
}