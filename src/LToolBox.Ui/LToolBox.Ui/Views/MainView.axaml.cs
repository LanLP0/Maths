using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using LToolBox.Ui.Service;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Views;

public sealed partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        FrameView.NavigationPageFactory = new NavigationFactory();
        NavigationService.Instance.SetFrame(FrameView);
        NavigationService.Instance.SetPageHeader(PageHeader);

        InitializeNavPages();
    }

    private void InitializeNavPages()
    {
        var pages = new NavViewModelBase[]
        {
            new CalcViewModel(),
            new MinMaxFracViewModel()
        };

        var menuItems = new List<NavigationViewItemBase>(pages.Length);

        Dispatcher.UIThread.Post(() =>
        {
            object? target = null;
            foreach (var page in pages)
            {
                var nvi = new NavigationViewItem
                {
                    Content = page.NavHeader,
                    Tag = page
                };
                menuItems.Add(nvi);

                if (page.IconKey is not null)
                    nvi.IconSource = (IconSource)this.FindResource(page.IconKey)!;

                // Restore previous page
                if (page.NavHeader == AppConfig.Instance.PageName)
                    target = nvi.Tag!;
            }

            NavView.MenuItemsSource = menuItems;

            target ??= menuItems[0].Tag!;
            NavigationService.Instance.NavigateFromContext(target);
        });
    }

    private void OnNavigationViewBackRequested(object sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            if (nvi.Tag is "theme-switch")
            {
                if (Application.Current!.ActualThemeVariant == ThemeVariant.Light)
                {
                    Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                    return;
                }

                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                return;
            }

            NavigationService.Instance.NavigateFromContext(nvi.Tag, e.RecommendedNavigationTransitionInfo);
        }
    }

    private void OnFrameViewNavigated(object sender, NavigationEventArgs e)
    {
        var page = e.Content as Control;
        var dc = page!.DataContext as NavViewModelBase;

        foreach (NavigationViewItem nvi in NavView.MenuItemsSource)
            if (nvi.Tag == dc)
                NavView.SelectedItem = nvi;
    }
}