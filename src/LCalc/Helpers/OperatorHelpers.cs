using Common.Results;

namespace LCalc.Helpers;

internal static class OperatorHelpers
{
    public static Result<long> ToInt64(double value, string operatorName)
    {
        var result = value.ToInt64();
        if (result.Success)
            return result;

        if (result.Exception is OverflowException)
            return new Exception($"Value(s) of operator {operatorName} must be between 2^63 and -2^63");

        return new Exception($"Value(s) of operator {operatorName} must be integer");
    }

    public static Result<int> ToInt(double value, string operatorName)
    {
        var result = value.ToInt();
        if (result.Success)
            return result;

        if (result.Exception is OverflowException)
            return new Exception($"Value(s) of operator {operatorName} must be between 2^31 and -2^31");

        return new Exception($"Value(s) of operator {operatorName} must be integer");
    }
}