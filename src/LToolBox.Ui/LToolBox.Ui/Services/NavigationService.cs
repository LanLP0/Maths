using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace LToolBox.Ui.Services;

public static class NavigationService
{
    public static Frame Frame { get; private set; }

    public static void SetFrame(Frame f)
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

    public static void NavigateFromContext(object dataContext, NavigationTransitionInfo transitionInfo = null)
    {
        Frame.NavigateFromObject(dataContext,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });
    }
}