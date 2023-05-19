using Common.Results;

namespace LCalc.Extension;

internal static class ResultExtension
{
    public static CalcResult MapToCalcResult(this Result<double> result)
    {
        return new CalcResult(result.Value, result.Exception);
    }

    public static CalcResult MapToCalcResult(this Result<bool> result)
    {
        return new CalcResult(result.Value, result.Exception);
    }
}