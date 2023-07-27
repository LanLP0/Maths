using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace LToolBox.Ui.Desktop;

public sealed class DesktopSuspensionDriver : ISuspensionDriver
{
    private string _file;

    public DesktopSuspensionDriver(string file)
    {
        _file = file;
    }

    public IObservable<object> LoadState()
    {
        var lines = File.ReadAllText(_file);
        var state = JsonSerializer.Deserialize<AppConfig>(lines, Global.SerializerOptions);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonSerializer.Serialize(state, Global.SerializerOptions);
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