using System.Globalization;

namespace LCalc;

public class CalcResult
{
    public CalcResult(Exception exception, string? steps = null)
    {
        Exception = exception;
        Steps = steps;
    }

    public CalcResult(bool result, string? steps = null)
    {
        Bool = result;
        Steps = steps;
    }

    public CalcResult(double result, string? steps = null)
    {
        Number = result;
        Steps = steps;
    }

    public bool IsBool => Bool.HasValue;
    public bool IsDouble => Number.HasValue;
    public bool Faulted => Exception is not null;
    public bool ContainSteps => Steps is not null;

    public bool? Bool { get; }

    public double? Number { get; }

    public Exception? Exception { get; }

    public string? Steps { get; private set; }

    internal CalcResult WithSteps(string steps)
    {
        Steps = steps;
        return this;
    }

    public string RenderValue(bool raw = false)
    {
        if (Faulted)
            return Exception!.Message;

        if (IsBool)
            return Bool!.Value.ToString();

        if (raw)
            return Number!.Value.ToString(CultureInfo.InvariantCulture);

        return Number!.Value.Humanize();
    }

    public string Render(bool raw = false)
    {
        if (Faulted) // Err
            return $"Error: {Exception!.Message}";

        if (IsBool) // Bool
        {
            if (ContainSteps)
                return $"{Steps}{Environment.NewLine}Result: {Bool!}";
            return $"Result: {Bool!}";
        }

        var result = Number!.Value;
        if (raw)
            result = Math.Round(result, 6);

        if (ContainSteps)
            return
                $"{Steps}{Environment.NewLine}Result: {(raw ? result.ToString(CultureInfo.InvariantCulture) : result.Humanize())}";

        return $"Result: {(raw ? result.ToString(CultureInfo.InvariantCulture) : result.Humanize())}";
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