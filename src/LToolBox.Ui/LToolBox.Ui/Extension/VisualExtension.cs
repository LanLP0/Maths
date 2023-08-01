using Avalonia;

namespace LToolBox.Ui.Extension;

public static class VisualExtension
{
    public static void Show(this Visual visual)
    {
        if (!visual.IsVisible)
            visual.IsVisible = true;
    }

    public static void Hide(this Visual visual)
    {
        if (visual.IsVisible)
            visual.IsVisible = false;
    }
}