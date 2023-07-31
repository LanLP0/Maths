using System;
using System.Reactive;
using System.Reactive.Linq;
using Android.Content;
using Newtonsoft.Json;
using ReactiveUI;

namespace LToolBox.Ui.Android;

public sealed class AndroidSuspensionDriver : ISuspensionDriver
{
    private ISharedPreferences _config;
    private const string ConfigKey = "config";

    public AndroidSuspensionDriver(ISharedPreferences config)
    {
        _config = config;
    }

    public IObservable<object> LoadState()
    {
        var s = _config.GetString(ConfigKey, string.Empty)!;
        var state = JsonConvert.DeserializeObject<AppState>(s);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonConvert.SerializeObject(state);
        
        var editor = _config.Edit()!;
        editor.PutString(ConfigKey, json);
        editor.Commit();
        
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        var editor = _config.Edit()!;
        editor.PutString(ConfigKey, string.Empty);
        editor.Commit();
        
        return Observable.Return(Unit.Default);
    }
}