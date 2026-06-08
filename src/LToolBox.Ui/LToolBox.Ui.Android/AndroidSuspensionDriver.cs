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
    private ISharedPreferences? _config;

    public void SetConfig(ISharedPreferences? config)
    {
        _config = config;
    }

    public IObservable<object> LoadState()
    {
        if (_config is null)
        {
            Log.Debug(MainActivity.LogTag, "LoadState - No config");
            throw new InvalidOperationException($"{nameof(AndroidSuspensionDriver)}.{nameof(LoadState)}");
            return Observable.Empty<object>();
        }
        
        Log.Debug(MainActivity.LogTag, "LoadState");
        var s = _config.GetString(ConfigKey, string.Empty)!;
        var state = JsonConvert.DeserializeObject<AppState>(s) ?? new AppState();
        return Observable.Return(state);
    }

    public IObservable<Unit> SaveState(object state)
    {
        if (_config is null)
        {
            Log.Debug(MainActivity.LogTag, "SaveState - No config");
            return Observable.Return(Unit.Default);
        }
        
        Log.Debug(MainActivity.LogTag, "SaveState");
        var json = JsonConvert.SerializeObject(state);

        var editor = _config.Edit()!;
        editor.PutString(ConfigKey, json);
        editor.Commit();

        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        if (_config is null)
        {
            Log.Debug(MainActivity.LogTag, "InvalidateState - No config");
            return Observable.Return(Unit.Default);
        }
        
        Log.Debug(MainActivity.LogTag, "InvalidateState");
        var editor = _config.Edit()!;
        editor.PutString(ConfigKey, string.Empty);
        editor.Commit();

        return Observable.Return(Unit.Default);
    }
}