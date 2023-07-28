using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LToolBox.Ui.Pages;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui;

public sealed class NavigationFactory : INavigationPageFactory
{
    public Control GetPage(Type srcType)
    {
        return null;
    }

    public Control GetPageFromObject(object target)
    {
        if (target is not NavViewModelBase nvmb)
            return null;

        AppState.Instance.PageName = nvmb.NavHeader;

        switch (target)
        {
            case CalcViewModel:
                return new CalcPage
                {
                    DataContext = target
                };
            case MinMaxFracViewModel:
                return new MinMaxFracPage
                {
                    DataContext = target
                };
            case SettingsViewModel:
                return new SettingsPage
                {
                    DataContext = target
                };
            default:
                return null;
        }
    }
}