namespace LCalc.Extension;

public static class FormatExtension
{
    public static bool IsValid(this Format format)
    {
        return (int)format is >= 0 and <= 4;
    }
}