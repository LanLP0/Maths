using System.Text;
using Common.Cli;
using LCalc;
using Spectre.Console;

namespace LToolBox.Tools;

internal sealed class LCalcTool : Tool
{
    private readonly StringBuilder _buffer = new();

    public LCalcTool(IAnsiConsole console) : base(console)
    {
    }

    public override string ToolName { get; } = "lcalc";

    public override string? HelpMsg { get; } = "A powerful calculator\nType [Yellow]q[/] to exit";

    public override void Execute()
    {
        var highlighter = new MathHighlighter();

        for (;;)
        {
            _buffer.Clear();

            var input = Console.Ask<string>("[white]Expression:[/]", clear: false, newLine: false,
                highlighter: highlighter);

            if (input is "q")
                return;

            var result = Calculator.CalcRaw(input!, out var raw);

            if (result.ContainSteps)
            {
                _buffer.Append(result.Steps);
                _buffer.Append(Environment.NewLine);
            }

            _buffer.Append("[white]");
            _buffer.Append(GetResultText(result));
            _buffer.Append("[/] ");
            _buffer.Append(result.RenderValue(raw));

            Console.MarkupLine(_buffer.ToString());
        }
    }

    private static string GetResultText(CalcResult result)
    {
        if (result.Faulted)
            return "Error:";

        return "Result:";
    }
}