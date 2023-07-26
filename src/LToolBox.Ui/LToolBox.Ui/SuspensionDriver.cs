using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using ReactiveUI;

namespace LToolBox.Ui;

public class SuspensionDriver : ISuspensionDriver
{
    private readonly string _file;

    public SuspensionDriver(string file)
    {
        _file = file;
    }

    public IObservable<object> LoadState()
    {
        var lines = File.ReadAllText(_file);
        var state = JsonSerializer.Deserialize<AppConfig>(lines);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonSerializer.Serialize(state);
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