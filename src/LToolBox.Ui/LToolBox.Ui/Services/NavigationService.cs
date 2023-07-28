using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Services;

public sealed class NavigationService
{
    private Frame _frame;
    public static NavigationService Instance { get; } = new();

    public void SetFrame(Frame f)
    {
        _frame = f;
    }

    public void NavigateFromContext(object dataContext, NavigationTransitionInfo transitionInfo = null)
    {
        _frame.NavigateFromObject(dataContext,
        new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
        });
    }
}