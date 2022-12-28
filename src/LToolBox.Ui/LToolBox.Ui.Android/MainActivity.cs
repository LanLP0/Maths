using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace LToolBox.Ui.Android;

[Activity(Label = "LToolBox.Ui.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon",
    LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public sealed class MainActivity : AvaloniaMainActivity
{
}