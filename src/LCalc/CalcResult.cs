using System.Globalization;
using Common.Maths;

namespace LCalc;

public struct CalcResult
{
    internal CalcResult(double? result, Exception? exception, Format format)
    {
        Number = result;
        Exception = exception;
        Format = format;
    }

    internal CalcResult(bool? result, Exception? exception, Format format)
    {
        Bool = result;
        Exception = exception;
        Format = format;
    }

    internal CalcResult(Exception exception, Format format)
    {
        Exception = exception;
        Format = format;
    }

    public bool IsBool => Bool.HasValue;
    public bool IsDouble => Number.HasValue;
    public bool IsNumber => IsDouble;
    public bool Faulted => Exception is not null;
    public bool ContainSteps => Steps is not null;

    public bool? Bool { get; }

    public double? Number { get; }

    public Exception? Exception { get; }

    public string? Steps { get; private set; }

    public Format Format { get; set; }

    internal CalcResult WithSteps(string steps)
    {
        Steps = steps;
        return this;
    }

    /// <summary>
    ///     Render just the value
    /// </summary>
    /// <returns></returns>
    public string RenderValue()
    {
        if (Faulted)
            return Exception!.Message;

        if (IsBool)
            return Bool!.Value.ToString();

        return RenderNumber(Number!.Value);
    }

    /// <summary>
    ///     Render the result (including the step(s))
    /// </summary>
    /// <returns></returns>
    public string Render()
    {
        if (Faulted)
            return $"Error: {Exception!.Message}";

        if (IsBool)
        {
            if (ContainSteps)
                return $"{Steps}{Environment.NewLine}Result: {Bool!}";
            return $"Result: {Bool!}";
        }

        var result = Number!.Value;

        if (ContainSteps)
            return $"{Steps}{Environment.NewLine}Result: {RenderNumber(result)}";

        return $"Result: {RenderNumber(result)}";
    }

    private string RenderNumber(double result)
    {
        return result.Format(Format);
    }

    public CalcResult WithFormat(Format format)
    {
        Format = format;
        return this;
    }

    internal static CalcResult Err(string message)
    {
        return new CalcResult(new Exception(message), default);
    }

    public override string ToString()
    {
        return Render();
    }
}