using Common.Results;

namespace LCalc.Extension;

internal static class ResultExtension
{
    public static CalcResult MapToCalcResult(this Result<double> result)
    {
        if (result.Faulted)
            return result.Exception!;

        return result.Value;
    }
}