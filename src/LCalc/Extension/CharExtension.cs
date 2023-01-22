namespace LCalc.Extension;

internal static class CharExtension
{
    public static bool IsDigit(this char c)
    {
        return (int)c is >= 48 and <= 57;
    }

    public static bool IsLowerLetter(this char c)
    {
        return (int)c is >= 97 and <= 122;
    }

    public static bool IsLetter(this char c)
    {
        var chrInt = (int)c;
        if (chrInt is not 65)
            chrInt -= 65;

        return chrInt is >= 97 and <= 122;
    }
}