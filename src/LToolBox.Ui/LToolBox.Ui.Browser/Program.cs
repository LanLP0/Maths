using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using LToolBox.Ui;
using LToolBox.Ui.Browser;

[assembly: SupportedOSPlatform("browser")]
internal partial class Program
{
    private static async Task Main(string[] args)
    {
        App.SetSuspensionDriver(new BrowserSuspensionDriver());
        
        await BuildAvaloniaApp()
            .WithInterFont()
            .UseReactiveUI()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}