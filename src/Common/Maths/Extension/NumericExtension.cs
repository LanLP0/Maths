using System.Globalization;
using Common.Results;

namespace Common.Maths.Extension;

public static class NumericExtension
{
    public static bool IsInt(this double value)
    {
        if (value % 1 > double.Epsilon)
            return false;

        return true;
    }
    
    public static Result<long> ToInt64(this double value)
    {
        if (value % 1 > double.Epsilon)
            return new Result<long>(new Exception($"Value {value} is not an integer"));
        if (double.IsNaN(value) || double.IsInfinity(value) || value is > long.MaxValue or < long.MinValue)
            return new Result<long>(new OverflowException($"Value {value} must be between 2^63 and -2^63"));
        
        return new Result<long>((long)value);
    }

    public static Result<int> ToInt(this double value)
    {
        if (value % 1 > double.Epsilon)
            return new Result<int>(new Exception($"Value {value} is not an integer"));
        if (double.IsNaN(value) || double.IsInfinity(value) || value is > int.MaxValue or < int.MinValue)
            return new Result<int>(new OverflowException($"Value {value} must be between 2^31 and -2^31"));
        
        return new Result<int>((int)value);
    }
    
    public static double ToRadians(this double angle)
    {
        return Math.PI / 180 * angle;
    }

    public static string Humanize(this double num, short maxDecimalPoints = 3)
    {
        switch (num)
        {
            case double.NaN:
                return "NaN";
            case double.PositiveInfinity:
            case double.NegativeInfinity:
                return "∞";
        }

        if (maxDecimalPoints < 2)
            return num.ToString(CultureInfo.InvariantCulture);

        var num1 = Math.Abs(num);
        double denominator = 1;
        for (var i = 0; i < maxDecimalPoints; i++)
        {
            if (num1 % 1 < double.Epsilon * 1000) break;

            num1 *= 10;
            denominator *= 10;
        }

        if (denominator is 1)
            return num.ToString(CultureInfo.InvariantCulture);

        if (num1 % 1 > double.Epsilon * 1000)
            return num.ToString(CultureInfo.InvariantCulture);

        if (denominator is <= 100 or >= 100_000)
            return num.ToString(CultureInfo.InvariantCulture);

        var gcd = Maths.GetGcd(num1, denominator);

        if (gcd is 1) return num.ToString(CultureInfo.InvariantCulture);

        denominator /= gcd;
        num1 = num * denominator;

        return $"{num1}/{denominator}";
    }
}