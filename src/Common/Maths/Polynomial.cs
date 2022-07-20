namespace Common.Cli.Maths;

internal static class Polynomial
{
    public static int Calc2(double a, double b, double c, out double? result1, out double? result2)
    {
        if (a is 0)
            return Calc1(b, c, out result1, out result2);
        
        var delta = b * b - 4 * a * c;
        switch (delta)
        {
            case < 0:
                result1 = null;
                result2 = null;
                return 0;
            case 0:
                result1 = -b / (2 * a);
                result2 = null;
                return 1;
            default:
                delta = Math.Sqrt(delta);
                result1 = (-b + delta) / (2 * a);
                result2 = (-b - delta) / (2 * a);
                return 2;
        }
    }
    
    private static int Calc1(double b, double c, out double? result1, out double? result2)
    {
        if (b is 0)
        {
            result1 = null;
            result2 = null;
            
            if (c is 0)
                return -1;

            return 0;
        }

        result1 = -c / b;
        result2 = null;
        return 1;
    }
}