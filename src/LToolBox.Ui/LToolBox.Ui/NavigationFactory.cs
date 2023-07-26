using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LToolBox.Ui.Pages;
using LToolBox.Ui.ViewModels;

namespace LToolBox.Ui;

public class NavigationFactory : INavigationPageFactory
{
    public Control GetPage(Type srcType)
    {
        return null;
    }

    public Control GetPageFromObject(object target)
    {
        if (target is CalcViewModel)
            return new CalcPage
            {
                DataContext = target
            };

        if (target is MinMaxFracViewModel)
            return new MinMaxFracPage
            {
                DataContext = target
            };

        return null;
    }
}