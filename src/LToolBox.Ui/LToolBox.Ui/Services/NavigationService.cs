using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace LToolBox.Ui.Services;

public static class NavigationService
{
    private static Frame _frame;

    public static void SetFrame(Frame f)
    {
        _frame = f;
    }

    public static bool GoBack()
    {
        if (!_frame.CanGoBack)
            return false;
        
        _frame.GoBack();
        return true;

    }

    public static void NavigateFromContext(object dataContext, NavigationTransitionInfo transitionInfo = null)
    {
        _frame.NavigateFromObject(dataContext,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });
    }
}