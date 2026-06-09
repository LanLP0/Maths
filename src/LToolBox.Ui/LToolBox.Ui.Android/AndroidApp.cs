using System;
using Android.Content;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace LToolBox.Ui.Android;

public sealed class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        App.LoggerConfiguration.WriteTo.Sink<LogCatSink>();
    
        App.SetSuspensionDriver(new AndroidSuspensionDriver());
    
        // App.SetSuspensionDriver(new MobileSuspensionDriver());
    
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }
}