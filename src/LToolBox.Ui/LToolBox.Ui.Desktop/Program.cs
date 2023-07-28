using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using Serilog;

namespace LToolBox.Ui.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
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

        App.SetSuspensionDriver(new DesktopSuspensionDriver(cfgFilePath));

        App.LoggerConfiguration.WriteTo.Trace();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI();
    }
}