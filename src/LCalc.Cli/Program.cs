using System.Text;
using LCalc;
using Spectre.Console;
using Common.Cli;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        Console.CancelKeyPress += (_, _) => Environment.Exit(0);

        if (args.Length is not 0)
        {
            var input = string.Join(' ', args);
            var result = Calculator.CalcFormatted(input);
            AnsiConsole.WriteLine(result);
            return;
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[red]This program needs to be run in interactive mode when run with no arguments[/]");
            return;
        }

        AnsiConsole.MarkupLine("Press [Yellow]Ctrl-C[/] to exit");

        var highlighter = new MathHighlighter();
        var buffer = new StringBuilder();

        for (;;)
        {
            buffer.Clear();

            var input = AnsiConsole.Console.Ask<string>("[white]Expression:[/]", clear: false, newLine: false,
                highlighter: highlighter);

            if (input is "q")
                return;

            var result = Calculator.CalcRaw(input!, out var raw);

            if (result.ContainSteps)
            {
                buffer.Append(result.Steps);
                buffer.Append(Environment.NewLine);
            }

            buffer.Append("[white]");
            buffer.Append(GetResultText(result));
            buffer.Append("[/] ");
            buffer.Append(result.RenderValue(raw));

            AnsiConsole.MarkupLine(buffer.ToString());
        }
    }
    
    public static string GetResultText(CalcResult result)
    {
        if (result.Faulted)
            return "Error:";

        return "Result:";
    }
}