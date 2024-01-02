using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Common.Cli.LineEditorCommands;
using RadLine;
using Spectre.Console;

namespace Common.Cli;

public static class AnsiConsoleExtension
{
    /// <summary>
    ///     Request the input from user and clear the request and return the cursor to previous location afterward
    /// </summary>
    /// <param name="console">The Spectre console</param>
    /// <param name="prompt">The prompt markup text</param>
    /// <param name="optional"></param>
    /// <param name="initialText">The initial text that will be shown</param>
    /// <param name="validators">The validators</param>
    /// <param name="clear">Clear the prompt afterward</param>
    /// <param name="newLine">Add a newline at the start</param>
    /// <param name="highlighter">The highlighter</param>
    /// <param name="history">The history entries</param>
    /// <typeparam name="T">The prompt result type</typeparam>
    /// <returns>The prompt result</returns>
    public static T? Ask<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IAnsiConsole console, string prompt, bool optional = false, string initialText = "",
        Validator<T?>? validators = null, bool clear = true, bool newLine = true, IHighlighter? highlighter = null,
        IReadOnlyList<string>? history = null)
    {
        if (newLine)
            console.WriteLine();

        var editor = new LineEditor
        {
            MultiLine = false,
            Prompt = new LineEditorPrompt(prompt),
            Text = initialText,
            Highlighter = highlighter
        };

        if (history is not null && history.Count > 0)
        {
            var historyCommand = new HistoryCommand(history);
            editor.KeyBindings.Add(ConsoleKey.UpArrow, () => historyCommand.GoUp());
            editor.KeyBindings.Add(ConsoleKey.DownArrow, () => historyCommand.GoDown());
        }

        var converter = TypeDescriptor.GetConverter(typeof(T));
        var hasErrorLine = false;

        for (;;)
        {
            var input = editor.ReadLine(CancellationToken.None).GetAwaiter().GetResult();

            // Nothing entered?
            if (string.IsNullOrEmpty(input))
            {
                if (optional)
                {
                    if (!clear)
                    {
                        console.Cursor.MoveUp();
                        console.Markup(prompt);
                        console.MarkupLine(" [gray]No Value[/]");
                    }

                    ClearAsk(console, clear, hasErrorLine, newLine);

                    return default;
                }

                hasErrorLine = true;
                console.ClearLine();
                console.Markup("[gray]A value is required[/]");

                console.Cursor.MoveUp();
                continue;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                hasErrorLine = true;
                console.ClearLine();
                console.Markup("[gray]Invalid value[/]");

                console.Cursor.MoveUp();
                continue;
            }

            try
            {
                var result = (T?)converter.ConvertFromInvariantString(input);

                if (validators is not null && !validators.RunUntilError(result, out var errorLine))
                {
                    hasErrorLine = true;
                    console.ClearLine();
                    errorLine = Markup.Escape(errorLine);
                    console.Markup($"[gray]{errorLine}[/]");

                    console.Cursor.MoveUp();
                    continue;
                }

                ClearAsk(console, clear, hasErrorLine, newLine);

                return result;
            }
            catch
            {
                hasErrorLine = true;
                console.ClearLine();
                console.Markup("[gray]Invalid value[/]");

                console.MoveCursorToStart();
                console.Cursor.MoveUp();
            }
        }
    }

    public static string ReadLine(this IAnsiConsole console)
    {
        var text = string.Empty;

        while (true)
        {
            var rawKey = console.ReadKey(true);
            if (rawKey is null) continue;

            var key = rawKey.Value;
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                {
                    return text;
                }
                case ConsoleKey.Backspace:
                {
                    if (text.Length > 0)
                    {
                        text = text.Substring(0, text.Length - 1);
                        console.Write("\b \b");
                    }

                    continue;
                }
            }

            if (char.IsControl(key.KeyChar))
                continue;

            text += key.KeyChar.ToString();
            var output = key.KeyChar.ToString();
            console.Write(output);
        }
    }

    /// <summary>
    ///     Reads a key from the console
    /// </summary>
    /// <param name="console">The IAnsiConsole</param>
    /// <param name="intercept">Whether or not to intercept the key</param>
    /// <returns>The key that was read</returns>
    public static ConsoleKeyInfo? ReadKey(this IAnsiConsole console, bool intercept)
    {
        return console.Input.ReadKey(intercept);
    }

    /// <summary>
    ///     Move the cursor to the start of the line
    /// </summary>
    /// <param name="console"></param>
    public static void MoveCursorToStart(this IAnsiConsole console)
    {
        console.Cursor.MoveLeft(console.Profile.Width);
    }

    /// <summary>
    ///     Clear the line
    /// </summary>
    /// <param name="console"></param>
    public static void ClearLine(this IAnsiConsole console)
    {
        console.MoveCursorToStart();
        console.Write(new string(' ', console.Profile.Width));
        console.MoveCursorToStart();
    }

    /// <summary>
    ///     Ask the user to choose an option then clear the line
    /// </summary>
    /// <param name="console">The IAnsiConsole</param>
    /// <param name="prompt">Markup prompt</param>
    /// <param name="options">The options</param>
    /// <param name="optional">Optional</param>
    /// <returns>The choosen choise</returns>
    public static string? ChooseOption(this IAnsiConsole console, string prompt, string[] options,
        bool optional = false, bool clear = true, bool newLine = true)
    {
        if (newLine)
            console.WriteLine();

        var editor = new LineEditor
        {
            MultiLine = false,
            Prompt = new LineEditorPrompt(prompt)
        };

        var hasErrorLine = false;

        for (;;)
        {
            var input = editor.ReadLine(CancellationToken.None).GetAwaiter().GetResult();

            // Nothing entered?
            if (string.IsNullOrEmpty(input))
            {
                if (optional)
                {
                    if (!clear)
                    {
                        console.Cursor.MoveUp();
                        console.Markup(prompt);
                        console.MarkupLine(" [gray]No Value[/]");
                    }

                    ClearAsk(console, clear, hasErrorLine, newLine);

                    return default;
                }

                hasErrorLine = true;
                console.ClearLine();
                console.Markup("[gray]A value is required[/]");

                console.Cursor.MoveUp();
                continue;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                hasErrorLine = true;
                console.ClearLine();
                console.Markup("[gray]Please enter a valid choice[/]");

                console.Cursor.MoveUp();
                continue;
            }

            if (!options.Contains(input))
            {
                hasErrorLine = true;
                console.ClearLine();
                console.Markup("[gray]Please enter a valid choice[/]");

                console.Cursor.MoveUp();
                continue;
            }

            ClearAsk(console, clear, hasErrorLine, newLine);

            return input;
        }
    }

    private static void ClearAsk(IAnsiConsole console, bool clear, bool hasErrorLine, bool lastMoveUp)
    {
        if (hasErrorLine)
            console.ClearLine();

        if (!clear)
            return;

        console.Cursor.MoveUp();
        console.ClearLine();
        if (lastMoveUp)
            console.Cursor.MoveUp();
    }
}