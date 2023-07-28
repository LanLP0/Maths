using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LToolBox.Ui.Services;
using LToolBox.Ui.ViewModels;
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
        ThemingService.Reload();

        if (!Design.IsDesignMode)
            InitializeSuspensionDriver();
        else
            ThemingService.SwitchToDarkMode();

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

    private void InitializeSuspensionDriver()
    {
        // If the platform is not supported
        _suspensionDriver ??= new DummySuspensionDriver();

        RxApp.SuspensionHost.CreateNewAppState = () => AppState.Instance;
        RxApp.SuspensionHost.SetupDefaultSuspendResume(_suspensionDriver);
        if (ApplicationLifetime is IControlledApplicationLifetime lt)
        {
            lt.Exit += (_, _) => SuspendHelper.SaveState();
            SuspendHelper.OnCreate();
        }

        // Load/Create the saved config
        AppState.Instance = RxApp.SuspensionHost.GetAppState<AppState>();
        RestoreAppState();
    }

    public static void InitializeLogger()
    {
        if (Logger is not null)
            return;
        
        Avalonia.Logging.Logger.Sink = new AvaloniaLogger();

        // Always write to trace
        LoggerConfiguration.WriteTo.Trace();

        Logger = LoggerConfiguration.CreateLogger();
    }

    private void RestoreAppState()
    {
        ThemingService.SetTheme(AppState.Instance.AppTheme);
    }
}