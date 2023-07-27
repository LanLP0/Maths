using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using ReactiveUI;
using Xamarin.Essentials;

namespace LToolBox.Ui.Mobile;

public sealed class MobileSuspensionDriver : ISuspensionDriver
{
    public IObservable<object> LoadState()
    {
        if (!Preferences.ContainsKey("config"))
            throw new Exception("config not set");
        
        var lines = Preferences.Get("config", string.Empty);
        var state = JsonSerializer.Deserialize<AppConfig>(lines);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonSerializer.Serialize(state);
        Preferences.Set("config", json);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        Preferences.Remove("config");
        return Observable.Return(Unit.Default);
    }
}