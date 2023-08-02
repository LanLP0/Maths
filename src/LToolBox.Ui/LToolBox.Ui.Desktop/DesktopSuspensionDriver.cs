using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using ReactiveUI;

namespace LToolBox.Ui.Desktop;

public sealed class DesktopSuspensionDriver : ISuspensionDriver
{
    private readonly string _file;

    public DesktopSuspensionDriver(string file)
    {
        _file = file;
    }

    public IObservable<object> LoadState()
    {
        var s = File.ReadAllText(_file);
        var state = JsonConvert.DeserializeObject<AppState>(s) ?? new AppState();
        return Observable.Return(state);
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonConvert.SerializeObject(state);
        File.WriteAllText(_file, json);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        if (File.Exists(_file))
            File.Delete(_file);
        return Observable.Return(Unit.Default);
    }
}