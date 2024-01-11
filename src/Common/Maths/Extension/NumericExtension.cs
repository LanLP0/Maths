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

    public static string Format(this double value, Format format)
    {
        switch (format)
        {
            case Common.Maths.Format.Raw:
                value = Math.Round(value, 6);
                return value.ToString(CultureInfo.InvariantCulture);
            case Common.Maths.Format.Hex:
                var isNeg = value < 0;

                value = Math.Round(Math.Abs(value));
                if (value > long.MaxValue)
                    return (isNeg ? "-" : "") + "0x..fffffff";
                var l = (long)value;

                return (isNeg ? "-0x" : "0x") + Convert.ToString(l, 16);
            case Common.Maths.Format.Octal:
                var isNeg1 = value < 0;

                value = Math.Round(Math.Abs(value));
                if (value > long.MaxValue)
                    return (isNeg1 ? "-" : "") + "0o..7777777";
                var l1 = (long)value;

                return (isNeg1 ? "-0o" : "0o") + Convert.ToString(l1, 8);
            case Common.Maths.Format.Binary:
                var isNeg2 = value < 0;

                value = Math.Round(Math.Abs(value));
                if (value > int.MaxValue)
                    return (isNeg2 ? "-" : "") + "0b..1111111";
                var i = (int)value;

                return (isNeg2 ? "-0b" : "0b") + Convert.ToString(i, 2);
            default:
                return value.Humanize();
        }
    }

    public static string ToStringRounded(this double value)
    {
        value = Math.Round(value, 3);
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string Humanize(this double value)
    {
        switch (value)
        {
            case double.NaN:
                return "NaN";
            case double.PositiveInfinity:
                return "∞";
            case double.NegativeInfinity:
                return "-∞";
        }

        var abs = Math.Abs(value);
        var denominator = 1;
        for (var i = 0; i < 4; i++)
        {
            if (abs.IsInt()) break;

            abs *= 10;
            denominator *= 10;
        }

        if (denominator <= 1000)
            return value.ToString(CultureInfo.InvariantCulture);

        var approx = Rational.Approximate(abs / 10000, 10E-8);
        if (approx.FractionPart.Denominator >= 1000)
        {
            value = Math.Round(value, 6);
            return value.ToString(CultureInfo.InvariantCulture);
        }

        if (value < 0)
            approx = -approx;

        if (!approx.FractionPart.IsZero)
            return approx.ToString();

        value = Math.Round(value, 6);
        return value.ToString(CultureInfo.InvariantCulture);
    }
}