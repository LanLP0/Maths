using System;
using Android.Util;
using Serilog.Core;
using Serilog.Events;

namespace LToolBox.Ui.Android;

public sealed class LogCatSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        Log.WriteLine(MapToLogPriority(logEvent.Level), MainActivity.LogTag, message);
    }

    private LogPriority MapToLogPriority(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => LogPriority.Verbose,
            LogEventLevel.Debug => LogPriority.Debug,
            LogEventLevel.Information => LogPriority.Info,
            LogEventLevel.Warning => LogPriority.Warn,
            LogEventLevel.Error => LogPriority.Error,
            LogEventLevel.Fatal => LogPriority.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}