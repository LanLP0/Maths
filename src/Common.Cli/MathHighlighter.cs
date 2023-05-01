using Common.Extensions;
using RadLine;
using Spectre.Console;

namespace LCalc.Cli;

public sealed class MathHighlighter : IHighlighter
{
    private static Style _grey = new Style(foreground: Color.Grey);
    private static Style _blue = new Style(foreground: Color.Blue);
    private static Style _yellow = new Style(foreground: Color.Yellow);
    
    private static string[] _blues = new[]
    {
        "+", "-", "*", "/", "^", "%", "!",
        "|", "&", "~", ">", "<", "="
    };
    
    public Style? Highlight(string token)
    {
        if (token is "(" or ")")
            return _grey;

        if (token.LettersOnly())
            return _yellow;

        if (_blues.Contains(token))
            return _blue;
        
        return null;
    }
}