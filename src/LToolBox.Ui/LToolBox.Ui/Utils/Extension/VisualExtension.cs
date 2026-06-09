using Avalonia;

namespace LToolBox.Ui.Utils.Extension;

public static class VisualExtension
{
    public static void Show(this Visual visual)
    {
        visual.IsVisible = true;
    }

    public static void Hide(this Visual visual)
    {
        visual.IsVisible = false;
    }
}