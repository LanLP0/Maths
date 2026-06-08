using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LToolBox.Ui.Pages;
using LToolBox.Ui.Pages.CalcPages;
using LToolBox.Ui.ViewModels;
using LToolBox.Ui.ViewModels.CalcPageViewModels;

namespace LToolBox.Ui;

public sealed class NavigationFactory : IFANavigationPageFactory
{
    public Control GetPage(Type srcType)
    {
        throw new NotImplementedException();
    }

    public Control GetPageFromObject(object target)
    {
        if (target is not NavViewModelBase nvmb)
            return null;

        AppState.Instance.PageName = nvmb.NavHeader;

        switch (target)
        {
            case CalcPageViewModel:
                return new CalcPage
                {
                    DataContext = target
                };
            case StepsPageViewModel:
                return new StepsPage
                {
                    DataContext = target
                };
            case HistoryPageViewModel:
                return new HistoryPage
                {
                    DataContext = target
                };
            case MinMaxFracPageViewModel:
                return new MinMaxFracPage
                {
                    DataContext = target
                };
            case PolynomialPageViewModel:
                return new PolynomialPage
                {
                    DataContext = target
                };
            case SettingsPageViewModel:
                return new SettingsPage
                {
                    DataContext = target
                };
            default:
                return null;
        }
    }
}