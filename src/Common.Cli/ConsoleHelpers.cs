using RadLine;
using Spectre.Console;

namespace Common.Cli;

public static class ConsoleHelpers
{
    public static string PromptAndClear(IAnsiConsole console, string prompt)
    {
        var result = Prompt(prompt);
        console.ClearLine();
        return result;
    }

    public static string Prompt(string prompt)
    {
        var editor = new LineEditor
        {
            MultiLine = false,
            Prompt = new LineEditorPrompt(prompt)
        };

        return editor.ReadLine(CancellationToken.None).ConfigureAwait(true).GetAwaiter().GetResult()!;
    }

    /// <summary>
    ///     Safely set the cursor position
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

        if (adj <= 0) return 0;

        Console.Write(new string('\n', adj));
        return -adj;
    }
}