using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Service;

public sealed class NavigationService
{
    private Frame _frame;
    private TextBlock _pageHeader;
    public static NavigationService Instance { get; } = new();

    public Control PreviousPage { get; set; }

    public void SetFrame(Frame f)
    {
        _frame = f;
    }

    public void SetPageHeader(TextBlock header)
    {
        _pageHeader = header;
    }

    public void NavigateFromContext(object dataContext, NavigationTransitionInfo transitionInfo = null)
    {
        var succeed = _frame.NavigateFromObject(dataContext,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });

        if (succeed)
            _pageHeader.Text = (dataContext as NavViewModelBase)!.NavHeader;
    }
}