using System.Text;
using Common.Cli;
using Spectre.Console;

namespace LCalc.Cli;

public static class CoreCli
{
    public static void RunLoop()
    {
        var highlighter = new MathHighlighter();
        var buffer = new StringBuilder();

        var prevAns = double.NaN;
        var history = new List<string>();
        for (;;)
        {
            var input = AnsiConsole.Console.Ask<string>("[white]Expression:[/]", clear: false, newLine: false,
                highlighter: highlighter, history: history)!;

            if (input is "q")
                return;

            AddToHistory(history, input);

            var result = Calculator.CalcRaw(input, prevAns: prevAns);

            if (result.ContainSteps)
            {
                buffer.Append(Markup.Escape(result.Steps!));
                buffer.Append(Environment.NewLine);
            }

            buffer.Append("[white]");
            buffer.Append(GetResultText(result));
            buffer.Append("[/] ");
            buffer.Append(result.RenderValue());

            AnsiConsole.MarkupLine(buffer.ToString());
            buffer.Clear();

            if (result.IsDouble)
                prevAns = result.Number!.Value;
        }
    }

    private static void AddToHistory(List<string> history, string input)
    {
        history.Insert(0, input);
        
        // Cap off at 10 history indexes
        if (history.Count > 10)
            history.RemoveAt(history.Count - 1);
    }

    private static string GetResultText(CalcResult result)
    {
        if (result.Faulted)
            return "Error:";

        return "Result:";
    }
}