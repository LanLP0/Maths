namespace LCalc.Extension;

public static class ExceptionExtension
{
    public static CalcResult ToCalcResult(this Exception exception)
    {
        return new CalcResult(exception, default);
    }
}