using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace LToolBox.Ui.Android;

[Activity(
    Label = "LToolBox",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity<App>
{
    public const string LogTag = "LToolBox.Ui";

    public override void Finish()
    {
        Log.Debug(LogTag, "App Finish");
        App.SuspendHelper.SaveState();
        base.Finish();
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        App.LoggerConfiguration.WriteTo.Sink<LogCatSink>();

        var cfg = GetPreferences(FileCreationMode.Private);
        App.SetSuspensionDriver(new AndroidSuspensionDriver(cfg));

        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }

    protected override void OnRestart()
    {
        Log.Debug(LogTag, "App Restart");
        App.SuspendHelper.OnUnpause();
        base.OnRestart();
    }

    protected override void OnDestroy()
    {
        Log.Debug(LogTag, "App Destroy");
        App.SuspendHelper.SaveState();
        base.OnDestroy();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        Log.Debug(LogTag, "App Create");

        if (savedInstanceState is null)
            App.SuspendHelper.OnCreate();
        else
            App.SuspendHelper.OnResume();

        base.OnCreate(savedInstanceState);
    }
}