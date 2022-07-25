using System.Text;
using System.Text.RegularExpressions;

namespace Common.Cli;

internal static class ConsoleHelpers
{
    public static void ClearLine(int? row = null)
    {
        var (currLeft, currTop) = Console.GetCursorPosition();
        row ??= currTop;

        Console.SetCursorPosition(0, row.Value);
        Console.Write(new string(' ', Console.WindowWidth));
        
        Console.SetCursorPosition(currLeft, currTop);
    }

    public static int? PromptIntAndClearLine(string prompt, int? row = null, int? lengthLimit = 5,
        bool isNegativeAllowed = true, string? defaultValue = null)
    {
        row ??= Console.CursorTop;
        
        ClearLine(row);

        Console.SetCursorPosition(0, row.Value);
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

            if (defaultValue[0] is '-')
            {
                isNegative = true;
            }
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
                    ClearLine(row);
                    Console.CursorLeft = 0;
                    return null;
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

                    ClearLine(row);
                    Console.CursorLeft = 0;
                    return int.Parse(buffer.ToString());
                }
            }
        }
    }
    
    public static int? PromptInt(string prompt, int? row = null, int? lengthLimit = 5,
        bool isNegativeAllowed = true, string defaultValue = "")
    {
        row ??= Console.CursorTop;
        
        ClearLine(row);

        Console.SetCursorPosition(0, row.Value);
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

            if (defaultValue[0] is '-')
            {
                isNegative = true;
            }
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
                    ClearLine(row);
                    Console.CursorLeft = 0;
                    return null;
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

                    Console.WriteLine();
                    return int.Parse(buffer.ToString());
                }
            }
        }
    }

    public static string? ChooseOption(string prompt, string[] options, int row, bool required = true)
    {
        Console.Write(prompt);

        var (currLeft, currTop) = Console.GetCursorPosition();

        var maxLength = options.Max(a => a.Length);
        var buffer = new StringBuilder();
        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input)
            {
                case {Key: ConsoleKey.Enter}:
                {
                    if (buffer.Length is 0)
                    {
                        if (required)
                        {
                            continue;
                        }

                        Console.WriteLine();
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
                case {Key: ConsoleKey.Backspace}:
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
        Console.SetCursorPosition(left, currTop);
        Console.Write(buffer);

        Console.CursorLeft = currLeft;
    }
    
    private static Lazy<Regex> colorBlockRegEx = new(
        ()=>  new Regex("\\[(?<color>.*?)\\](?<text>[^[]*)\\[/\\k<color>\\]", RegexOptions.IgnoreCase), 
        isThreadSafe: true);
    
    /// <summary>
    /// Allows a string to be written with embedded color values using:
    /// This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
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

        int at = text.IndexOf('[');
        int at2 = text.IndexOf(']');
        if (at is -1 || at2 <= at)
        {
            WriteLine(text, baseTextColor);
            return;
        }

        while (true)
        {
            var match = colorBlockRegEx.Value.Match(text);
            if (match.Length < 1)
            {
                Write(text, baseTextColor);
                break;
            }

            // write up to expression
            Write(text.Substring(0, match.Index), baseTextColor);

            // strip out the expression
            string highlightText = match.Groups["text"].Value;
            string colorVal = match.Groups["color"].Value;

            Write(highlightText, colorVal);

            // remainder of string
            text = text.Substring(match.Index + match.Value.Length);
        }

        Console.WriteLine();
    }
    
    /// <summary>
    /// Allows a string to be written with embedded color values using:
    /// This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
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

        int at = text.IndexOf('[');
        int at2 = text.IndexOf(']');
        if (at is -1 || at2 <= at)
        {
            WriteLine(text, baseTextColor);
            return;
        }

        while (true)
        {
            var match = colorBlockRegEx.Value.Match(text);
            if (match.Length < 1)
            {
                Write(text, baseTextColor);
                break;
            }

            // write up to expression
            Write(text.Substring(0, match.Index), baseTextColor);

            // strip out the expression
            string highlightText = match.Groups["text"].Value;
            string colorVal = match.Groups["color"].Value;

            Write(highlightText, colorVal);

            // remainder of string
            text = text.Substring(match.Index + match.Value.Length);
        }
    }
    
    /// <summary>
    /// WriteLine with color
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    public static void WriteLine(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
        {
            var oldColor = Console.ForegroundColor;
            if (color == oldColor)
                Console.WriteLine(text);
            else
            {
                Console.ForegroundColor = color.Value;
                Console.WriteLine(text);
                Console.ForegroundColor = oldColor;
            }
        }
        else
            Console.WriteLine(text);
    }

    /// <summary>
    /// Write with color
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    public static void Write(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
        {
            var oldColor = Console.ForegroundColor;
            if (color == oldColor)
                Console.Write(text);
            else
            {
                Console.ForegroundColor = color.Value;
                Console.Write(text);
                Console.ForegroundColor = oldColor;
            }
        }
        else
            Console.Write(text);
    }

    /// <summary>
    /// Writes out a line with color specified as a string
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
        {
            Write(text);
        }
        else
        {
            Write(text, col);
        }
    }
}