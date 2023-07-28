using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace LToolBox.Ui.Browser;

public sealed class BrowserSuspensionDriver : ISuspensionDriver
{
    public IObservable<object> LoadState()
    {
        var json = JavascriptStateManager.Load();
        if (string.IsNullOrEmpty(json))
            throw new Exception();
        
        var state = JsonSerializer.Deserialize<AppState>(json, Global.SerializerOptions);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonSerializer.Serialize(state, Global.SerializerOptions);
        JavascriptStateManager.Save(json);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        JavascriptStateManager.Invalidate();
        return Observable.Return(Unit.Default);
    }
}