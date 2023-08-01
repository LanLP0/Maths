using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LToolBox.Ui.Services;
using LToolBox.Ui.Views;
using ReactiveUI;
using Serilog;
using Serilog.Core;

namespace LToolBox.Ui;

public sealed class App : Application
{
    private static ISuspensionDriver? _suspensionDriver;
    public static LoggerConfiguration LoggerConfiguration { get; } = new();
    public static Logger? Logger { get; private set; }

    public static GenericSuspendHelper SuspendHelper { get; } = new();

    public static void SetSuspensionDriver(ISuspensionDriver suspensionDriver)
    {
        if (_suspensionDriver is not null)
            return;

        _suspensionDriver = suspensionDriver;
    }

    public override void Initialize()
    {
        InitializeLogger();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (!Design.IsDesignMode)
            InitializeSuspensionDriver();
        else
            ThemingService.SetTheme(AppTheme.Dark, true);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView();

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeSuspensionDriver()
    {
        // If the platform is not supported
        _suspensionDriver ??= new DummySuspensionDriver();

        RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(_suspensionDriver);
        if (ApplicationLifetime is IControlledApplicationLifetime lt)
        {
            lt.Exit += (_, _) => SuspendHelper.SaveState();
            SuspendHelper.OnCreate();
        }

        // Load/Create the saved config
        AppState.Instance = RxApp.SuspensionHost.GetAppState<AppState>();
        RestoreTheme();
    }

    public static void InitializeLogger()
    {
        if (Logger is not null)
            return;

#if DEBUG
        Avalonia.Logging.Logger.Sink = new AvaloniaLogger();
        LoggerConfiguration.WriteTo.Trace();
        LoggerConfiguration.WriteTo.Debug();
#endif

        Logger = LoggerConfiguration.CreateLogger();
    }

    private void RestoreTheme()
    {
        try
        {
            ThemingService.SetTheme(AppState.Instance.AppTheme, true);
            ThemingService.SetAccentColor(AppState.Instance.AccentColor);
        }
        catch (Exception e)
        {
            Logger?.Error("{error}", e);
        }
    }
}