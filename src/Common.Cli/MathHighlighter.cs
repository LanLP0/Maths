using System.Diagnostics;
using System.Text;
using Common.Extensions;
using RadLine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Common.Cli;

public sealed class MathHighlighter : IHighlighter
{
    private static readonly Style BracesStyle = new(Color.Blue);
    private static readonly Style TokenStyle = new(Color.Aqua);
    private static readonly Style TextStyle = new(Color.DarkOliveGreen1);

    private static readonly string[] Tokens =
    [
        "+", "-", "*", "/", "^", "%", "!",
        "|", "&", "~", ">", "<", "=", "[", "]"
    ];

    public IRenderable BuildHighlightedText(string text)
    {
        var paragraph = new Paragraph();

        foreach (var token in StringTokenizer.Tokenize(text))
        {
            // if (double.TryParse(token, out _))
            //     paragraph.Append(token, NumberStyle);
            if (token.All(char.IsAsciiLetter))
                paragraph.Append(token, TextStyle);
            else if (Tokens.Contains(token))
                paragraph.Append(token, TokenStyle);
            else if (token is "(" or ")")
                paragraph.Append(token, BracesStyle);
            else
                paragraph.Append(token);
        }
        
        return paragraph;
    }
    
    private static class StringTokenizer
    {
        public static IEnumerable<string> Tokenize(string text)
        {
            var buffer = new StringBuilder();
            foreach (var character in text)
            {
                if (char.IsLetterOrDigit(character))
                {
                    buffer.Append(character);
                }
                // else if (character == '.' && int.TryParse(buffer.ToString(), out _))
                // {
                //     buffer.Append(character);
                // }
                else
                {
                    if (buffer.Length > 0)
                    {
                        yield return buffer.ToString();
                        buffer.Clear();
                    }

                    yield return new string(character, 1);
                }
            }

            if (buffer.Length > 0)
            {
                yield return buffer.ToString();
            }
        }
    }
}