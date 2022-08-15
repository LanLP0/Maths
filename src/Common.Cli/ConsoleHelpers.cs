using System.Text;
using System.Text.RegularExpressions;

namespace Common.Cli;

public static class ConsoleHelpers
{
    private static readonly Lazy<Regex> ColorBlockRegEx = new(
        () => new Regex("\\[(?<color>.*?)\\](?<text>[^[]*)\\[/\\k<color>\\]", RegexOptions.IgnoreCase),
        true);

    public static int ClearLine(int? top = null)
    {
        var (currLeft, currTop) = Console.GetCursorPosition();
        top ??= currTop;

        var adj = SafeSetCursorPosition(0, top.Value);
        Console.Write(new string(' ', Console.WindowWidth));

        SafeSetCursorPosition(currLeft, currTop);

        return adj;
    }

    public static (int? result, int adj) PromptIntAndClearLine(string prompt, int? top = null, int? lengthLimit = 5,
        bool isNegativeAllowed = true, string? defaultValue = null)
    {
        top ??= Console.CursorTop;
        var (currLeft, currTop) = Console.GetCursorPosition();

        var result = PromptIntInternal(prompt, top, lengthLimit, isNegativeAllowed, defaultValue!);

        Console.CursorTop--;
        ClearLine();
        
        Console.SetCursorPosition(currLeft, currTop + result.adj);

        return (result.result, result.adj);
    }

    public static int? PromptInt(string prompt, int? top = null, int? lengthLimit = 5,
        bool isNegativeAllowed = true, string defaultValue = "") =>
        PromptIntInternal(prompt, top, lengthLimit, isNegativeAllowed, defaultValue).Item1;

    private static (int? result, int adj) PromptIntInternal(string prompt, int? top = null, int? lengthLimit = 5,
        bool isNegativeAllowed = true, string? defaultValue = null)
    {
        top ??= Console.CursorTop;
        var adj = 0;

        var adjTmp = ClearLine(top);
        adj += adjTmp;
        top += adjTmp;

        adj += SafeSetCursorPosition(0, top.Value);

        Console.Write(prompt);
        var left = Console.CursorLeft;

        StringBuilder buffer = new();
        var pos = 0;
        var isNegative = false;
        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            buffer.Append(defaultValue);
            Console.Write(defaultValue);
            pos += defaultValue.Length;

            if (defaultValue[0] is '-') isNegative = true;
        }

        for (;;)
        {
            var input = Console.ReadKey(true);
            switch (input.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.H:
                {
                    if (pos is 0)
                        break;

                    Console.CursorLeft--;
                    pos--;
                    break;
                }
                case ConsoleKey.RightArrow:
                case ConsoleKey.L:
                {
                    if (pos == buffer.Length)
                        break;

                    Console.CursorLeft++;
                    pos++;
                    break;
                }
                case ConsoleKey.Backspace:
                {
                    if (pos is 0)
                        break;

                    if (pos is 1 && isNegative)
                    {
                        isNegative = false;
                        buffer.Remove(0, 1);
                        pos--;
                        Console.CursorLeft--;
                        ReRender(buffer, left);
                        break;
                    }

                    buffer.Remove(--pos, 1);
                    Console.CursorLeft--;
                    ReRender(buffer, left);
                    break;
                }
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                case ConsoleKey.NumPad0:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                {
                    if (isNegative)
                    {
                        if (buffer.Length >= lengthLimit + 1)
                            break;
                    }
                    else
                    {
                        if (buffer.Length >= lengthLimit)
                            break;
                    }

                    buffer.Insert(pos, input.KeyChar);
                    Console.CursorLeft++;
                    ReRender(buffer, left);
                    pos++;
                    break;
                }
                case ConsoleKey.Subtract:
                case ConsoleKey.OemMinus:
                {
                    if (!isNegativeAllowed)
                        break;

                    if (isNegative)
                    {
                        isNegative = false;
                        buffer.Remove(0, 1);
                        pos--;
                        Console.CursorLeft--;
                        ReRender(buffer, left);
                        break;
                    }

                    isNegative = true;
                    buffer.Insert(0, '-');
                    pos++;
                    Console.CursorLeft++;
                    ReRender(buffer, left);
                    break;
                }
                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                {
                    buffer.Clear();
                    buffer.Append("$null");
                    ReRender(buffer, left);

                    if (Console.CursorTop + 1 == Console.BufferHeight)
                        adj--;
                    
                    Console.WriteLine();
                    return (null, adj);
                }
                case ConsoleKey.Enter:
                {
                    if (isNegative)
                    {
                        if (buffer.Length is 1)
                            break;
                    }
                    else
                    {
                        if (buffer.Length is 0)
                            break;
                    }

                    if (Console.CursorTop + 1 == Console.BufferHeight)
                        adj--;
                    
                    Console.WriteLine();
                    return (int.Parse(buffer.ToString()), adj);
                }
            }
        }
    }

    public static string? ChooseOption(string prompt, string[] options, bool required = true)
    {
        Console.Write(prompt);

        var maxLength = options.Max(a => a.Length);
        var buffer = new StringBuilder();
        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input)
            {
                case { Key: ConsoleKey.Enter }:
                {
                    if (buffer.Length is 0)
                    {
                        if (required) continue;

                        Console.WriteLine("$null");
                        return null;
                    }

                    var selection = buffer.ToString();
                    if (options.Contains(selection))
                    {
                        Console.WriteLine();
                        return selection;
                    }

                    break;
                }
                default:
                {
                    if (input.KeyChar is '\0')
                        continue;

                    if (buffer.Length >= maxLength)
                        continue;

                    buffer.Append(char.ToLower(input.KeyChar));
                    Console.Write(char.ToLower(input.KeyChar));

                    break;
                }
                case { Key: ConsoleKey.Backspace }:
                {
                    if (buffer.Length is 0)
                        continue;

                    Console.CursorLeft--;
                    Console.Write(' ');
                    Console.CursorLeft--;

                    buffer.Remove(buffer.Length - 1, 1);

                    break;
                }
            }
        }
    }

    private static void ReRender(StringBuilder buffer, int left)
    {
        var (currLeft, currTop) = Console.GetCursorPosition();
        Console.CursorLeft = left;

        Console.Write(new string(' ', Math.Clamp(Console.WindowWidth - left, 0, int.MaxValue)));
        SafeSetCursorPosition(left, currTop);
        Console.Write(buffer);

        Console.CursorLeft = currLeft;
    }

    /// <summary>
    /// Safely set the cursor position
    /// </summary>
    /// <returns>The offset of <c>top</c></returns>
    public static int SafeSetCursorPosition(int left, int top)
    {
        left = Math.Clamp(left, 0, Console.BufferWidth - 1);
        top = top < 0 ? 0 : top;
        
        var adj = SafeGoToTop(top);
        Console.CursorLeft = left;
        
        return adj;
    }

    private static int SafeGoToTop(int top)
    {
        top = top < 0 ? 0 : top;

        if (top < Console.BufferHeight)
        {
            Console.CursorTop = top;
            return 0;
        }

        var adj = top - Console.CursorTop;
        
        if (adj <= 0)
        {
            return 0;
        }

        Console.Write(new string('\n', adj));
        return -adj;
    }

    /// <summary>
    ///     Allows a string to be written with embedded color values using:
    ///     This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="baseTextColor">Base text color</param>
    public static void WriteEmbeddedColorLine(string text, ConsoleColor? baseTextColor = null)
    {
        baseTextColor ??= Console.ForegroundColor;

        if (string.IsNullOrEmpty(text))
        {
            WriteLine(string.Empty);
            return;
        }

        var at = text.IndexOf('[');
        var at2 = text.IndexOf(']');
        if (at is -1 || at2 <= at)
        {
            WriteLine(text, baseTextColor);
            return;
        }

        while (true)
        {
            var match = ColorBlockRegEx.Value.Match(text);
            if (match.Length < 1)
            {
                Write(text, baseTextColor);
                break;
            }

            // write up to expression
            Write(text.Substring(0, match.Index), baseTextColor);

            // strip out the expression
            var highlightText = match.Groups["text"].Value;
            var colorVal = match.Groups["color"].Value;

            Write(highlightText, colorVal);

            // remainder of string
            text = text.Substring(match.Index + match.Value.Length);
        }

        Console.WriteLine();
    }

    /// <summary>
    ///     Allows a string to be written with embedded color values using:
    ///     This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="baseTextColor">Base text color</param>
    public static void WriteEmbeddedColor(string text, ConsoleColor? baseTextColor = null)
    {
        baseTextColor ??= Console.ForegroundColor;

        if (string.IsNullOrEmpty(text))
        {
            WriteLine(string.Empty);
            return;
        }

        var at = text.IndexOf('[');
        var at2 = text.IndexOf(']');
        if (at is -1 || at2 <= at)
        {
            WriteLine(text, baseTextColor);
            return;
        }

        while (true)
        {
            var match = ColorBlockRegEx.Value.Match(text);
            if (match.Length < 1)
            {
                Write(text, baseTextColor);
                break;
            }

            // write up to expression
            Write(text.Substring(0, match.Index), baseTextColor);

            // strip out the expression
            var highlightText = match.Groups["text"].Value;
            var colorVal = match.Groups["color"].Value;

            Write(highlightText, colorVal);

            // remainder of string
            text = text.Substring(match.Index + match.Value.Length);
        }
    }

    /// <summary>
    ///     WriteLine with color
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    public static void WriteLine(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
        {
            var oldColor = Console.ForegroundColor;
            if (color == oldColor)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.ForegroundColor = color.Value;
                Console.WriteLine(text);
                Console.ForegroundColor = oldColor;
            }
        }
        else
        {
            Console.WriteLine(text);
        }
    }

    /// <summary>
    ///     Write with color
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    public static void Write(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
        {
            var oldColor = Console.ForegroundColor;
            if (color == oldColor)
            {
                Console.Write(text);
            }
            else
            {
                Console.ForegroundColor = color.Value;
                Console.Write(text);
                Console.ForegroundColor = oldColor;
            }
        }
        else
        {
            Console.Write(text);
        }
    }

    /// <summary>
    ///     Writes out a line with color specified as a string
    /// </summary>
    /// <param name="text">Text to write</param>
    /// <param name="color">A console color. Must match ConsoleColors collection names (case insensitive)</param>
    public static void Write(string text, string color)
    {
        if (string.IsNullOrEmpty(color))
        {
            Write(text);
            return;
        }

        if (!EnumHelpers.TryParseFast(color, out var col))
            Write(text);
        else
            Write(text, col);
    }
}