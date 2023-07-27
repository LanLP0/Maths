using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace LToolBox.Ui.Browser;

public sealed class BrowserSuspensionDriver : ISuspensionDriver
{
    private JsonSerializerOptions _options;

    public BrowserSuspensionDriver()
    {
        _options = new JsonSerializerOptions
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false
        };
    }

    public IObservable<object> LoadState()
    {
        var json = JavascriptStateStorage.Load();
        if (string.IsNullOrEmpty(json))
            throw new Exception();
        
        var state = JsonSerializer.Deserialize<AppConfig>(json, _options);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state)
    {
        var json = JsonSerializer.Serialize(state, _options);
        JavascriptStateStorage.Save(json);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState()
    {
        JavascriptStateStorage.Invalidate();
        return Observable.Return(Unit.Default);
    }
}