namespace Common.Extensions;

internal static class StringExtension
{
    /// <summary>
    ///     Check if a string contains only ASCII letters
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool LettersOnly(this string s)
    {
        foreach (var c in s)
        {
            var isAsciiLetter = (uint)((c | 0x20) - 'a') <= 'z' - 'a';
            if (!isAsciiLetter)
                return false;
        }

        return true;
    }
}