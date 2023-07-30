using System.Globalization;

namespace LCalc;

public sealed class CalcResult
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
        var raw = Format is Format.Raw;
        if (raw)
            result = Math.Round(result, 6);

        if (ContainSteps)
            return $"{Steps}{Environment.NewLine}Result: {RenderNumber(result)}";

        return $"Result: {RenderNumber(result)}";
    }

    private string RenderNumber(double result)
    {
        switch (Format)
        {
            case Format.Raw:
                return result.ToString(CultureInfo.InvariantCulture);
            case Format.Hex:
                var isNeg = result < 0;
                
                result = Math.Round(Math.Abs(result));
                if (result > long.MaxValue)
                    return (isNeg ? "-" : "") + "0x..fffffff";
                var l = (long)result;

                return (isNeg ? "-0x" : "0x") + Convert.ToString(l, 16);
            case Format.Octal:
                var isNeg1 = result < 0;
                
                result = Math.Round(Math.Abs(result));
                if (result > long.MaxValue)
                    return (isNeg1 ? "-" : "") + "0o..7777777";
                var l1 = (long)result;

                return (isNeg1 ? "-0o" : "0o") + Convert.ToString(l1, 8);
            case Format.Binary:
                var isNeg2 = result < 0;
                
                result = Math.Round(Math.Abs(result));
                if (result > int.MaxValue)
                    return (isNeg2 ? "-" : "") + "0b..1111111";
                var i = (int)result;

                return (isNeg2 ? "-0b" : "0b") + Convert.ToString(i, 2);
            default:
                return result.Humanize();
        }
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