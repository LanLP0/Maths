using Common.Maths;
using Common.Results;

namespace LCalc.Extension;

internal static class ResultExtension
{
    public static CalcResult MapToCalcResult(this Result<double> result, Format format)
    {
        return new CalcResult(result.Value, result.Exception, format);
    }

    public static CalcResult MapToCalcResult(this Result<bool> result, Format format)
    {
        return new CalcResult(result.Value, result.Exception, format);
    }
}