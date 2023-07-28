using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;
using ColorChangedEventArgs = Avalonia.Controls.ColorChangedEventArgs;

namespace LToolBox.Ui.Views;

public sealed partial class MainView : UserControl
{
    private MainViewViewModel _vm;
    
    public MainView()
    {
        InitializeComponent();

        _vm = (MainViewViewModel)DataContext!;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        FrameView.NavigationPageFactory = new NavigationFactory();
        NavigationService.Instance.SetFrame(FrameView);

        InitializeNavPages();
    }

    private void InitializeNavPages()
    {
        var pages = new NavViewModelBase[]
        {
            new CalcViewModel(),
            new MinMaxFracViewModel(),
            new SettingsViewModel()
        };

        var menuItems = new List<NavigationViewItemBase>(2);
        var footerItems = new List<NavigationViewItemBase>(2);

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
            NavigationService.Instance.NavigateFromContext(target);
        });
    }

    private void OnNavigationViewBackRequested(object sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is not NavigationViewItem nvi)
            return;

        if (nvi.Tag is "theme-switch") ThemingService.SwitchThemeMode();

        NavigationService.Instance.NavigateFromContext(nvi.Tag, e.RecommendedNavigationTransitionInfo);
    }

    private void OnFrameViewNavigated(object sender, NavigationEventArgs e)
    {
        var page = e.Content as Control;
        var nvmb = page!.DataContext as NavViewModelBase;
        PageHeader.Text = nvmb!.NavHeader;

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
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        NavView.PaneDisplayMode = e.NewSize.Width < 400
            ? NavigationViewPaneDisplayMode.LeftMinimal
            : NavigationViewPaneDisplayMode.LeftCompact;
    }
}