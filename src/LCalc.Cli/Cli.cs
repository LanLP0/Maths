using System.Text;
using Common.Cli;
using Spectre.Console;

namespace LCalc.Cli;

public static class Cli
{
    public static void RunLoop()
    {
        var highlighter = new MathHighlighter();
        var buffer = new StringBuilder();

        var prevAns = double.NaN;
        for (;;)
        {
            buffer.Clear();

            var input = AnsiConsole.Console.Ask<string>("[white]Expression:[/]", clear: false, newLine: false,
                highlighter: highlighter);

            if (input is "q")
                return;

            var result = Calculator.CalcRaw(input!, prevAns: prevAns);

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

            if (result.IsDouble)
                prevAns = result.Number!.Value;
        }
    }

    private static string GetResultText(CalcResult result)
    {
        if (result.Faulted)
            return "Error:";

        return "Result:";
    }
}