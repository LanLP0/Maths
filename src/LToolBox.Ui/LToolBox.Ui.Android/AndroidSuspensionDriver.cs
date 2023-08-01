using System;
using System.Reactive;
using System.Reactive.Linq;
using Android.Content;
using Android.Util;
using Newtonsoft.Json;
using ReactiveUI;

namespace LToolBox.Ui.Android;

public sealed class AndroidSuspensionDriver : ISuspensionDriver
{
    private const string ConfigKey = "config";
    private readonly ISharedPreferences _config;

    public AndroidSuspensionDriver(ISharedPreferences config)
    {
        _config = config;
    }

    public IObservable<object> LoadState()
    {
        Log.Debug(MainActivity.LogTag, "LoadState");
        var s = _config.GetString(ConfigKey, string.Empty)!;
        var state = JsonConvert.DeserializeObject<AppState>(s);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        Log.Debug(MainActivity.LogTag, "SaveState");
        var json = JsonConvert.SerializeObject(state);

        var editor = _config.Edit()!;
        editor.PutString(ConfigKey, json);
        editor.Commit();

        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        Log.Debug(MainActivity.LogTag, "InvalidateState");
        var editor = _config.Edit()!;
        editor.PutString(ConfigKey, string.Empty);
        editor.Commit();

        return Observable.Return(Unit.Default);
    }
}