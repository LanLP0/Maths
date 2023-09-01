using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui.Views;

public sealed partial class MainView : UserControl
{
    private readonly MainViewViewModel _vm;

    public MainView()
    {
        _vm = new MainViewViewModel();
        DataContext = _vm;

        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        FrameView.NavigationPageFactory = new NavigationFactory();
        NavigationService.SetFrame(FrameView);

        InitializeNavPages();
    }

    private void InitializeNavPages()
    {
        var pages = new NavViewModelBase[]
        {
            new CalcPageViewModel(),
            new MinMaxFracPageViewModel(),
            new SettingsPageViewModel()
        };

        var menuItems = new List<NavigationViewItemBase>(2);
        var footerItems = new List<NavigationViewItemBase>(2);

        object? target = null;
        foreach (var page in pages)
        {
            var nvi = new NavigationViewItem
            {
                Content = page.NavHeader,
                Tag = page
            };
            if (page.IsFooter)
                footerItems.Add(nvi);
            else
                menuItems.Add(nvi);

            if (page.IconKey is not null)
                nvi.IconSource = (IconSource)this.FindResource(page.IconKey)!;

            // Restore previous page
            if (page.NavHeader == AppState.Instance.PageName)
                target = nvi.Tag!;
        }

        // Quick option to switch theme mode
        footerItems.Insert(0, new NavigationViewItem
        {
            Content = "Switch Theme",
            Tag = "theme-switch",
            IconSource = (IconSource)this.FindResource("DarkThemeIcon")!,
            SelectsOnInvoked = false
        });

        NavView.MenuItemsSource = menuItems;
        NavView.FooterMenuItemsSource = footerItems;

        target ??= menuItems[0].Tag!;
        NavigationService.NavigateFromContext(target);
    }

    private void OnNavigationViewBackRequested(object sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is not NavigationViewItem nvi)
            return;

        if (nvi.Tag is "theme-switch")
        {
            ThemingService.SwitchThemeMode();
            return;
        }

        NavigationService.NavigateFromContext(nvi.Tag, e.RecommendedNavigationTransitionInfo);
    }

    private void OnFrameViewNavigated(object sender, NavigationEventArgs e)
    {
        var page = e.Content as Control;
        var nvmb = page!.DataContext as NavViewModelBase;
        _vm.HeaderText = nvmb!.NavHeader;

        foreach (NavigationViewItem nvi in NavView.MenuItemsSource)
        {
            if (nvi.Tag != nvmb)
                continue;

            _vm.SelectedItem = nvi;
            return;
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItemsSource)
        {
            if (nvi.Tag != nvmb)
                continue;

            _vm.SelectedItem = nvi;
            return;
        }

        _vm.SelectedItem = null!;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        NavView.PaneDisplayMode = e.NewSize.Width < 400
            ? NavigationViewPaneDisplayMode.LeftMinimal
            : NavigationViewPaneDisplayMode.LeftCompact;
    }
}