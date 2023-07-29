using System;
using Avalonia.Logging;

namespace LToolBox.Ui;

public sealed class AvaloniaLogger : ILogSink
{
    public bool IsEnabled(LogEventLevel level, string area)
    {
        return App.Logger?.IsEnabled(MapLogLevel(level)) ?? false;
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        App.Logger?.Write(MapLogLevel(level), "{source}-{name}: {message}",
            source?.GetType().FullName, area, messageTemplate);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate,
        params object?[] propertyValues)
    {
        App.Logger?.Write(MapLogLevel(level), "{name}-{area}: " + messageTemplate,
            source?.GetType().FullName, area, propertyValues);
    }

    private Serilog.Events.LogEventLevel MapLogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => Serilog.Events.LogEventLevel.Verbose,
            LogEventLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogEventLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogEventLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogEventLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogEventLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}