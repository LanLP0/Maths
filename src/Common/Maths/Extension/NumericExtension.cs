using System.Globalization;
using Common.Results;
using Rationals;

namespace Common.Maths.Extension;

internal static class NumericExtension
{
    public static bool IsInt(this double value)
    {
        return value % 1 <= double.Epsilon;
    }

    public static Result<long> ToInt64(this double value)
    {
        if (!value.IsInt())
            return new Result<long>(new Exception($"Value {value} is not an integer"));
        if (double.IsNaN(value) || double.IsInfinity(value) || value is > long.MaxValue or < long.MinValue)
            return new Result<long>(new OverflowException($"Value {value} must be between 2^63 and -2^63"));

        return new Result<long>((long)value);
    }

    public static Result<int> ToInt(this double value)
    {
        if (!value.IsInt())
            return new Result<int>(new Exception($"Value {value} is not an integer"));
        if (double.IsNaN(value) || double.IsInfinity(value) || value is > int.MaxValue or < int.MinValue)
            return new Result<int>(new OverflowException($"Value {value} must be between 2^31 and -2^31"));

        return new Result<int>((int)value);
    }

    public static double ToRadians(this double angle)
    {
        return Math.PI / 180 * angle;
    }

    public static string Humanize(this double num)
    {
        switch (num)
        {
            case double.NaN:
                return "NaN";
            case double.PositiveInfinity:
            case double.NegativeInfinity:
                return "∞";
        }

        var num1 = Math.Abs(num);
        double denominator = 1;
        for (var i = 0; i < 4; i++)
        {
            if (num1.IsInt()) break;

            num1 *= 10;
            denominator *= 10;
        }

        if (denominator <= 100)
            return num.ToString(CultureInfo.InvariantCulture);

        var approx = Rational.Approximate(num, 10E-8);
        if (approx.FractionPart.Denominator >= 1000)
        {
            num = Math.Round(num, 6);
            return num.ToString(CultureInfo.InvariantCulture);
        }

        if (!approx.FractionPart.IsZero)
            return approx.ToString();

        num = Math.Round(num, 6);
        return num.ToString(CultureInfo.InvariantCulture);
    }
}