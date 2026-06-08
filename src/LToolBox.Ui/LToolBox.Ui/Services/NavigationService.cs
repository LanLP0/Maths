using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace LToolBox.Ui.Services;

public static class NavigationService
{
    public static FAFrame Frame { get; private set; }

    public static void SetFrame(FAFrame f)
    {
        Frame = f;
    }

    public static bool GoBack()
    {
        if (!Frame.CanGoBack)
            return false;

        Frame.GoBack();
        return true;
    }

    public static void NavigateFromContext(object dataContext, FANavigationTransitionInfo? transitionInfo = null)
    {
        Frame.NavigateFromObject(dataContext,
            new FAFrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new FASuppressNavigationTransitionInfo()
            });
    }
}