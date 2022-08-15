namespace Common.Maths.Extension;

public static class NumericExtension
{
    public static double ToRadians(this double angle)
    {
        return Math.PI / 180 * angle;
    }

    public static string ToFraction(this double num, short decimalPoints = 20)
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
        for (var i = 0; i < decimalPoints; i++)
        {
            if (num1 % 1 < double.Epsilon * 1000) break;

            num1 *= 10;
            denominator *= 10;
        }

        if (denominator is 1)
            return num.ToString();

        num1 = num * denominator;

        if (Math.Abs(num1 % 1) > double.Epsilon * 1000)
            return num.ToString();

        var gcd = Maths.GetGcd(num1, denominator);

        if (gcd is 1) return num.ToString();

        denominator /= gcd;
        num1 /= gcd;

        if (Math.Max(denominator, num1) > 1_000_000)
            return num.ToString();

        return $"{num1}/{denominator}";
    }
}