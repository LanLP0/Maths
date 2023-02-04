using System.Globalization;

namespace LCalc;

public class CalcResult
{
    private string? _steps;

    public CalcResult(Exception exception, string? steps = null)
    {
        AsException = exception;
        _steps = steps;
    }

    public CalcResult(bool result, string? steps = null)
    {
        AsBool = result;
        _steps = steps;
    }

    public CalcResult(double result, string? steps = null)
    {
        AsDouble = result;
        _steps = steps;
    }

    public bool IsBool => AsBool.HasValue;
    public bool IsDouble => AsDouble.HasValue;
    public bool Faulted => AsException is not null;
    public bool ContainSteps => _steps is not null;

    public bool? AsBool { get; }

    public double? AsDouble { get; }

    public Exception? AsException { get; }

    public string? GetStep()
    {
        return _steps;
    }

    internal CalcResult WithSteps(string steps)
    {
        _steps = steps;
        return this;
    }

    public string Render(bool rawValue = false)
    {
        if (Faulted) // Err
            return $"Error: {AsException!.Message}";

        if (IsBool) // Bool
        {
            if (ContainSteps)
                return $"{_steps}{Environment.NewLine}Result: {AsBool!}";
            return $"Result: {AsBool!}";
        }

        var result = AsDouble!.Value;
        if (rawValue)
            result = Math.Round(result, 6);

        if (ContainSteps)
            return
                $"{_steps}{Environment.NewLine}Result: {(rawValue ? result.ToString(CultureInfo.InvariantCulture) : result.Humanize())}";

        return $"Result: {(rawValue ? result.ToString(CultureInfo.InvariantCulture) : result.Humanize())}";
    }

    public static implicit operator CalcResult(Exception exception)
    {
        return new CalcResult(exception);
    }

    public static implicit operator CalcResult(bool result)
    {
        return new CalcResult(result);
    }

    public static implicit operator CalcResult(double result)
    {
        return new CalcResult(result);
    }

    public override string ToString()
    {
        return Render();
    }
}