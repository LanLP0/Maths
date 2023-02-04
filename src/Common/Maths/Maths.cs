namespace Common.Maths;

public static class Maths
{
    public static List<int> GetFact(int value)
    {
        var result = new List<int>();

        for (var i = 2; i < value;)
        {
            while (value % i is 0)
            {
                value /= i;
                result.Add(i);
            }

            i = GetNextPrime(i);
        }

        result.Add(value);

        return result;
    }

    public static int GetNextPrime(int value = 0)
    {
        if (value < 2)
            return 2;

        if (value % 2 is 0)
            value++;
        else
            value += 2;

        for (;;)
        {
            if (IsPrimeInternal(value))
                return value;

            value += 2;
        }
    }

    private static bool IsPrimeInternal(int value)
    {
        var root = (int)Math.Sqrt(value);
        for (var a = 3; a <= root; a += 2)
            if (value % a is 0)
                return false;

        return true;
    }

    public static bool IsPrime(int value)
    {
        if (value <= 2)
            return false;

        if (value % 2 is 0)
            return false;

        var root = (int)Math.Sqrt(value);
        for (var a = 3; a <= root; a += 2)
            if (value % a is 0)
                return false;

        return true;
    }

    public static double FastGcd(double a, double b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        double gcd = 0;

        while (b is not 0)
        {
            gcd = b;
            b = a % b;
            a = gcd;
        }

        return gcd;
    }

    public static double FastLcm(double a, double b)
    {
        return a * b / FastGcd(a, b);
    }
}