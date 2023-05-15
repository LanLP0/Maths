using Common.Extensions;
using RadLine;
using Spectre.Console;

namespace Common.Cli;

public sealed class MathHighlighter : IHighlighter
{
    private static readonly Style Grey = new(foreground: Color.Grey);
    private static readonly Style Blue = new(foreground: Color.Blue);
    private static readonly Style Yellow = new(foreground: Color.Yellow);
    
    private static string[] _keys =
    {
        "+", "-", "*", "/", "^", "%", "!",
        "|", "&", "~", ">", "<", "=", "[", "]"
    };
    
    public Style? Highlight(string token)
    {
        if (token is "(" or ")")
            return Grey;

        if (token.LettersOnly())
            return Yellow;

        if (_keys.Contains(token))
            return Blue;
        
        return null;
    }
}