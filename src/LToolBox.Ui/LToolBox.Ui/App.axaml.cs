using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LToolBox.Ui.ViewModels;
using LToolBox.Ui.Views;
using ReactiveUI;

namespace LToolBox.Ui;

public sealed class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Persistence config

        // Default to app.cfg in bin directory
        // if the config directory is readonly
        var cfgFilePath = "app.cfg";
        try
        {
            var cfgDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LanLP", "LToolBox");
            Directory.CreateDirectory(cfgDirectory);
            cfgFilePath = Path.Join(cfgDirectory, "app.cfg");
        }
        catch
        {
            // ignored
        }

        var suspension = new AutoSuspendHelper(ApplicationLifetime);
        RxApp.SuspensionHost.CreateNewAppState = () => new AppConfig();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new SuspensionDriver(cfgFilePath));
        suspension.OnFrameworkInitializationCompleted();

        // Load/Create the saved config
        AppConfig.Instance = RxApp.SuspensionHost.GetAppState<AppConfig>();
        RestoreAppState();

        Current!.ActualThemeVariantChanged += OnActualThemeVariantChanged;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewViewModel()
            };
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        AppConfig.Instance.AppTheme = Current!.ActualThemeVariant;
    }

    private void RestoreAppState()
    {
        Current!.RequestedThemeVariant = AppConfig.Instance.AppTheme;
    }
}