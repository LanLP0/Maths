using System;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using ReactiveUI;

namespace LToolBox.Ui.Browser;

public sealed class BrowserSuspensionDriver : ISuspensionDriver
{
    public IObservable<object> LoadState()
    {
        var json = JavascriptStateManager.Load();
        if (string.IsNullOrEmpty(json))
            throw new Exception();

        var state = JsonConvert.DeserializeObject<AppState>(json);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonConvert.SerializeObject(state);
        JavascriptStateManager.Save(json);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        JavascriptStateManager.Invalidate();
        return Observable.Return(Unit.Default);
    }
}